using System;
using System.Collections.Generic;
using System.Text;

namespace ShapeFlow.ModelToCode
{
    public static class DotNetScalarTypeSelector
    {
        public static string SqlTypeToCSharpType(string sqlType)
        {
            sqlType = sqlType.ToLowerInvariant();

            switch (sqlType)
            {
                case "varchar":
                case "nvarchar":
                case "nchar":
                case "ntext":
                case "text":
                case "char":
                    return "string";

                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return "DateTime";

                case "time":
                    return "TimeSpan";

                case "datetimeoffset":
                    return "DateTimeOffset";

                case "money":
                case "smallmoney":
                case "decimal":
                case "numeric":
                    return "decimal";

                case "int":
                    return "int";

                case "bigint":
                    return "long";

                case "bit":
                    return "boolean";

                case "image":
                case "binary":
                case "rowversion":
                case "timestamp":
                case "varbinary":
                    return "byte[]";

                case "float":
                    return "double";

                case "real":
                    return "float";

                case "tinyint":
                    return "byte";

                case "uniqueidentifier":
                    return "Guid";

                case "xml":
                    return "string";


                default:
                    return "object";
            }
        }
    }
}
