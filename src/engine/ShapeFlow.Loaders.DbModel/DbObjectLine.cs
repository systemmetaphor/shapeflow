namespace ShapeFlow.Loaders.DbModel
{
    class DbObjectLine
    {
        public string ObjectSchema { get; set; }

        public string ObjectName { get; set; }

        public string PropertyName { get; set; }

        public int? PropertyPosition { get; set; }

        public string PropertyDefaultValue { get; set; }

        public string PropertySqlDataType { get; set; }

        public int? PropertyLength { get; set; }

        public int? PropertyPrecision { get; set; }

        public int? PropertyPrecisionRadix { get; set; }

        public int? Scale { get; set; }

        public int? DateTimePrecision { get; set; }

        public string SqlIsNullable { get; set; }
    }
}
