namespace ShapeFlow.Loaders.DbModel
{
    public class ColumnModel
    {
        public string Name { get; set; }

        public int? Position { get; set; }

        public string DefaultValue { get; set; }

        public string SqlDataType { get; set; }

        public string PropertyDotNetType
        {
            get
            {
                var sqlType = (SqlDataType ?? string.Empty).ToLower();
                switch(sqlType)
                {
                    case "varchar":
                        return "string";

                    case "nvarchar":
                        return "string";

                    case "date":
                        return "DateTime";

                    case "decimal":
                        return "decimal";

                    case "int":
                        return "int";

                    case "bigint":
                        return "long";

                    default:
                        return "object";
                }
            }
        }                

        public int? Length { get; set; }

        public int? Precision { get; set; }

        public int? PrecisionRadix { get; set; }

        public int? Scale { get; set; }

        public int? DateTimePrecision { get; set; }

        public string SqlIsNullable { get; set; }
    }
}