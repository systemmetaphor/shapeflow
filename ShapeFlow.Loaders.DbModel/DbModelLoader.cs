using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;
using ShapeFlow.Infrastructure;
using ShapeFlow.ModelDriven;
using ShapeFlow.ModelDriven.Loaders;
using ShapeFlow.ModelDriven.Models;

namespace ShapeFlow.Loaders.DbModel
{
    public class DbModelLoader : IModelLoader
    {
        private const string TableMetadataQuery = @"
SELECT 
	col.TABLE_SCHEMA as [ObjectSchema], 
	col.TABLE_NAME as [ObjectName], 
	COLUMN_NAME as [PropertyName], 
	ORDINAL_POSITION as [PropertyPosition],
	COLUMN_DEFAULT as [PropertyDefaultValue], 
	DATA_TYPE as [PropertySqlDataType], 
	CHARACTER_MAXIMUM_LENGTH as [PropertyLength],
    NUMERIC_PRECISION  as [PropertyPrecision], 
	NUMERIC_PRECISION_RADIX as [PropertyPrecisionRadix], 
	NUMERIC_SCALE as [Scale],
    DATETIME_PRECISION as [DateTimePrecision],
    IS_NULLABLE as [SqlIsNullable]
FROM 
	INFORMATION_SCHEMA.COLUMNS col
LEFT JOIN
	INFORMATION_SCHEMA.TABLES tbl
ON
	tbl.TABLE_NAME = col.TABLE_NAME AND tbl.TABLE_SCHEMA = col.TABLE_SCHEMA
WHERE
	(tbl.TABLE_TYPE = 'BASE TABLE')
";

        private readonly ILoggingService _loggingService;

        public string Name => "DbModelLoader";

        public ModelFormat Format => ModelFormat.Clr;

        public DbModelLoader(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public ModelContext LoadModel(ModelDeclaration declaration)
        {
            var result = new EntityModelRoot();
            
            var tableName = declaration.GetParameter("tableName");
            
            var databaseInfo = new DatabaseInfo
            {
                Server = declaration.GetParameter("server"),
                Name = declaration.GetParameter("db"),
                User = declaration.GetParameter("user"),
                Password = declaration.GetParameter("password")
            };

            var query = TableMetadataQuery;

            _loggingService.Debug($"Loading metadata from database {databaseInfo.Name} on {databaseInfo.Server} instance.");
            
            if(!string.IsNullOrWhiteSpace(tableName))
            {
                query = string.Concat(query, " and tbl.TABLE_NAME = @TableName");
            }

            using (var connection = new SqlConnection(SqlHelper.GetConnectionString(databaseInfo)))
            {
                var lines = connection.Query<DbObjectLine>(TableMetadataQuery, new { TableName = tableName });
                var grouped = lines.GroupBy(l => l.ObjectName);
                foreach(var g in grouped)
                {
                    var m = new EntityModel
                    {
                        ObjectName = SafeName(g.Key)
                    };

                    m.AddProperties(g.OrderBy(p => p.PropertyPosition).Select(p => new PropertyModel
                    {
                        DateTimePrecision = p.DateTimePrecision,
                        PropertyDefaultValue = p.PropertyDefaultValue,
                        PropertyLength = p.PropertyLength,
                        PropertyName = p.PropertyName,
                        PropertyPosition = p.PropertyPosition,
                        PropertyPrecision = p.PropertyPrecision,
                        PropertyPrecisionRadix = p.PropertyPrecisionRadix,
                        PropertySqlDataType = p.PropertySqlDataType,
                        Scale = p.Scale,
                        SqlIsNullable = p.SqlIsNullable
                    }));

                    result.AddModels(new[] { m });
                }
            }

            _loggingService.Debug($"Loaded { result.Entities.Count() } models.");

            return new ModelContext(declaration, new DbModel(result, ModelFormat.Clr,declaration.ModelName, declaration.Tags));
        }

        public bool ValidateArguments(ModelDeclaration context)
        {
            return true;
        }

        private static string SafeName(string name)
        {
            return name.Replace(' ', '_');
        }

        static class SqlHelper
        {
            internal const string ParameterPrefix = "@";
            internal const string OpenSquares = "[";
            internal const string CloseSquares = "]";

            private static readonly SqlCommandBuilder Builder = new SqlCommandBuilder();

            internal static string GetConnectionString(DatabaseInfo databaseInfo)
            {
                return GetConnectionString(databaseInfo.Server, databaseInfo.User, databaseInfo.Password, databaseInfo.Name);
            }

            internal static string GetConnectionString(string server, string user, string password, string name)
            {
                string connectionString;

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                {
                    connectionString = $"Data Source={server};Initial Catalog={name};User ID={user};Password={password};MultipleActiveResultSets=True";
                }
                else
                {
                    connectionString = $"Data Source={server};Initial Catalog={name};Integrated Security=True;MultipleActiveResultSets=True";
                }

                return connectionString;
            }

            internal static string QuoteIdentifier(string value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (value.StartsWith(ParameterPrefix, StringComparison.Ordinal))
                {
                    return value;
                }
                else if (IsQuoted(value))
                {
                    return value;
                }
                else
                {
                    return Builder.QuoteIdentifier(value);
                }
            }

            internal static bool IsQuoted(string value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (value.Length < 2)
                {
                    return false;
                }

                return value.StartsWith(OpenSquares, StringComparison.Ordinal)
                       && value.EndsWith(CloseSquares, StringComparison.Ordinal);
            }

            internal static string UnquoteIdentifier(string value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (IsQuoted(value))
                {
                    return value.Substring(1, value.Length - 2);
                }

                return value;
            }

            internal static SqlConnection GetConnection(DatabaseInfo database)
            {
                return GetConnection(database.Server, database.User, database.Password, database.Name);
            }

            internal static SqlConnection GetConnection(string dbServer, string dbUser, string dbPassword, string dbName)
            {
                var conStr = GetConnectionString(dbServer, dbUser, dbPassword, dbName);

                return new SqlConnection(conStr);
            }
        }

        class DatabaseInfo
        {
            public DatabaseInfo()
            {
            }

            public string Name
            {
                get;
                set;
            }

            public string Server
            {
                get;
                set;
            }

            public string User
            {
                get;
                set;
            }

            public string Password
            {
                get;
                set;
            }
        }
    }

    public class DbModel : Model
    {
        public DbModel(EntityModelRoot root, ModelFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = root;
        }

        public EntityModelRoot Root { get; }

        public override object GetModelInstance()
        {
            return Root;
        }
    }
}
