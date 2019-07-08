using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ExcelDataReader;

namespace ShapeFlow.Loaders.Excel
{
    public class XlsImportGenerators
    {
        private static readonly Regex nonDigitOrLetterReplacer = new Regex("[^a-zA-Z0-9]");

        public IEnumerable<XlsColumnInfo> GetColumnInfo(string fileName, int linesToSkip = 0)
        {
            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataset = reader.AsDataSet();
                    var table = dataset.Tables[0];
                    var cols = table.Columns.OfType<System.Data.DataColumn>().ToArray();
                    if (cols.Length > 0)
                    {

                        var results = new List<XlsColumnInfo>();
                        foreach (var col in cols)
                        {
                            var nameOnFile = col.ColumnName;
                            if (string.IsNullOrWhiteSpace(nameOnFile))
                            {
                                break;
                            }

                            var columnName = DatabaseFriendlyName(nameOnFile);

                            if (columnName.Equals("ID", StringComparison.OrdinalIgnoreCase))
                            {
                                columnName = "IdOnFile";
                            }

                            string typeOnDatabase = "NVARCHAR";
                            string lengthOnDatabase = "(256)";
                            string nullability = "NULL";
                            string isNullDefault = "''";

                            Type type;
                            string csharpType;

                            switch (col.DataType.Name)
                            {
                                case "Int64":

                                    type = typeof(long);
                                    csharpType = "long";
                                    typeOnDatabase = "BIGINT";
                                    lengthOnDatabase = string.Empty;
                                    isNullDefault = "0";
                                    break;

                                case "Decimal":

                                    type = typeof(decimal);
                                    csharpType = "decimal";
                                    typeOnDatabase = "DECIMAL";
                                    lengthOnDatabase = "(21,6)";
                                    isNullDefault = "0";
                                    break;

                                case "DateTime":
                                    type = typeof(DateTime);
                                    csharpType = "DateTime";
                                    typeOnDatabase = "DATETIME";
                                    lengthOnDatabase = string.Empty;
                                    isNullDefault = "0";
                                    break;

                                default:
                                    type = typeof(string);
                                    csharpType = "string";
                                    break;
                            }

                            results.Add(new XlsColumnInfo
                            {
                                NameOnWorksheet = nameOnFile,
                                NameOnDatabase = columnName,
                                NameOnCSharp = columnName, // for now
                                TypeOnDatabase = typeOnDatabase,
                                LengthOnDatabase = lengthOnDatabase,
                                Nullability = nullability,
                                ImportAs = type,
                                CSharpType = csharpType,
                                IsNullDefaultOnDatabase = isNullDefault
                            });
                        }

                        return results;
                    }
                }
            }

            return Enumerable.Empty<XlsColumnInfo>();
        }

        public static string DatabaseFriendlyName(string originalName)
        {
            if (string.IsNullOrWhiteSpace(originalName))
            {
                return originalName;
            }

            if (originalName.EndsWith("?"))
            {
                originalName = originalName.Substring(0, originalName.Length - 1);
            }

            if (originalName.StartsWith("?"))
            {
                originalName = originalName.Substring(1, originalName.Length - 1);
            }

            if (originalName.EndsWith(":"))
            {
                originalName = originalName.Substring(0, originalName.Length - 1);
            }

            if (originalName.StartsWith(":"))
            {
                originalName = originalName.Substring(1, originalName.Length - 1);
            }

            var validText = nonDigitOrLetterReplacer.Replace(originalName.Trim(), ReplaceInvalidChar);
            return validText;
        }

        private static string ReplaceInvalidChar(Match match)
        {
            switch (match.Value)
            {
                case "ã": return "a";
                case "é": return "e";
                case "ê": return "e";
                case "á": return "a";
                case "à": return "a";
                case "ç": return "c";
                case "ó": return "o";

                case "Ã": return "A";
                case "É": return "E";
                case "Ê": return "E";
                case "Á": return "A";
                case "À": return "A";
                case "Ç": return "C";
                case "Ó": return "O";

                case "%": return "PERCENTAGE";
            }

            return "_";
        }
    }
}