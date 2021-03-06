﻿using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ShapeFlow.Infrastructure;
using ShapeFlow;
using ShapeFlow.Declaration;
using ShapeFlow.Loaders;
using ShapeFlow.Shapes;

namespace ShapeFlow.Loaders.DbModel
{
    public class DbModelLoader : ILoader
    {
        private const string TableMetadataQuery = @"
; WITH PRIMARY_KEYS (TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME)
as(
    SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
        ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
        AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
) 
SELECT 
	col.TABLE_SCHEMA as [ObjectSchema], 
	col.TABLE_NAME as [ObjectName], 
	col.COLUMN_NAME as [PropertyName], 
	ORDINAL_POSITION as [PropertyPosition],
	COLUMN_DEFAULT as [PropertyDefaultValue], 
	DATA_TYPE as [PropertySqlDataType], 
	CHARACTER_MAXIMUM_LENGTH as [PropertyLength],
    NUMERIC_PRECISION  as [PropertyPrecision], 
	NUMERIC_PRECISION_RADIX as [PropertyPrecisionRadix], 
	NUMERIC_SCALE as [Scale],
    DATETIME_PRECISION as [DateTimePrecision],
    IS_NULLABLE as [SqlIsNullable],
    IIF(pks.COLUMN_NAME is null, 0, 1) as [IsPrimaryKey]
FROM 
	INFORMATION_SCHEMA.COLUMNS col
LEFT JOIN
	INFORMATION_SCHEMA.TABLES tbl
ON
	tbl.TABLE_NAME = col.TABLE_NAME AND tbl.TABLE_SCHEMA = col.TABLE_SCHEMA
LEFT JOIN
    PRIMARY_KEYS as pks
ON
    pks.TABLE_NAME = col.TABLE_NAME AND pks.TABLE_SCHEMA = col.TABLE_SCHEMA and pks.COLUMN_NAME = col.COLUMN_NAME
WHERE
	(tbl.TABLE_TYPE = 'BASE TABLE')
";
                

        public string Name => "DbModelLoader";

        public ShapeFormat Format => ShapeFormat.Clr;

        public async Task<ShapeContext> Load(ShapeDeclaration declaration)
        {
            var result = new DatabaseModelRoot();
            
            var tableName = declaration.GetParameter("tableName");

            var connectionName = declaration.GetParameter("connectionName");
            DatabaseInfo databaseInfo;

            if (!string.IsNullOrWhiteSpace(connectionName))
            {
                var connectionString = DbConnectionDiscovery.GetConnection(connectionName);
                databaseInfo = DatabaseInfo.FromConnectionString(connectionString);
            }
            else
            {
                databaseInfo = new DatabaseInfo
                {
                    Server = declaration.GetParameter("server"),
                    Name = declaration.GetParameter("db"),
                    User = declaration.GetParameter("user"),
                    Password = declaration.GetParameter("password")
                };
            }

            var query = TableMetadataQuery;

            AppTrace.Verbose($"Loading metadata from database {databaseInfo.Name} on {databaseInfo.Server} instance.");
            
            if(!string.IsNullOrWhiteSpace(tableName))
            {
                query = string.Concat(query, " and tbl.TABLE_NAME = @TableName");
            }

            using (var connection = new SqlConnection(SqlHelper.GetConnectionString(databaseInfo)))
            {
                var lines = await connection.QueryAsync<DbObjectLine>(query, new { TableName = tableName });
                var grouped = lines.GroupBy(l => l.ObjectName);
                foreach(var g in grouped)
                {
                    if (!g.Any())
                    {
                        continue;
                    }

                    var m = new TableModel
                    {
                        ObjectName = SafeName(g.Key),
                        ObjectSchema = SafeName(g.First().ObjectSchema)
                    };

                    m.AddColumn(g.OrderBy(p => p.PropertyPosition).Select(p => new ColumnModel
                    {
                        DateTimePrecision = p.DateTimePrecision,
                        DefaultValue = p.PropertyDefaultValue,
                        Length = p.PropertyLength,
                        Name = p.PropertyName,
                        Position = p.PropertyPosition,
                        Precision = p.PropertyPrecision,
                        PrecisionRadix = p.PropertyPrecisionRadix,
                        SqlDataType = p.PropertySqlDataType,
                        Scale = p.Scale,
                        SqlIsNullable = p.SqlIsNullable
                    }));

                    result.AddModels(new[] { m });
                }
            }

            AppTrace.Verbose($"Loaded { result.Entities.Count() } models.");

            return new ShapeContext(declaration, new DatabaseModelShape(result, ShapeFormat.Clr,declaration.Name, declaration.Tags));
        }

        public Task Save(ShapeContext context)
        {
            throw new NotImplementedException();
        }

        public ShapeContext Create(ShapeDeclaration decl)
        {
            throw new NotImplementedException();
        }

        public ShapeContext CreateSet(ShapeDeclaration decl)
        {
            throw new NotImplementedException();
        }

        public bool ValidateArguments(ShapeDeclaration context)
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

            public static DatabaseInfo FromConnectionString(string connectionString)
            {
                try
                {
                    var builder = new SqlConnectionStringBuilder(connectionString);

                    return new DatabaseInfo
                    {
                        Name = builder.InitialCatalog,
                        Server = builder.DataSource,
                        User = builder.UserID,
                        Password = builder.Password
                    };
                }
                catch (Exception e)
                {
                    AppTrace.Warning($"Could not parse the provided connection string: { e.Message } ");
                    return null;
                }
            }
        }
    }
}
