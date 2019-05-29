namespace ShapeFlow.Loaders.DbModel
{
    public class PropertyModel
    {
        public string PropertyName { get; set; }
        public int? PropertyPosition { get; set; }
        public string PropertyDefaultValue { get; set; }
        public string PropertySqlDataType { get; set; }

        public string PropertyDotNetType
        {
            get
            {
                var sqlType = (PropertySqlDataType ?? string.Empty).ToLower();
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

        public string DotNetNullableSign
        {
            get
            {
                var isNullable = (SqlIsNullable ?? string.Empty).ToLower();
                switch(isNullable)
                {
                    case "yes":
                        return "?";
                    default:
                        return "";
                }
            }
        }

        public string JavaScriptPropertyName
        {
            get
            {
                return this.PropertyName.ToCamelCase();
            }
        }

        public int? PropertyLength { get; set; }
        public int? PropertyPrecision { get; set; }
        public int? PropertyPrecisionRadix { get; set; }
        public int? Scale { get; set; }
        public int? DateTimePrecision { get; set; }
        public string SqlIsNullable { get; set; }
    }
}