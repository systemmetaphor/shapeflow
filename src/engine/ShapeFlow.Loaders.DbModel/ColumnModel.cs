namespace ShapeFlow.Loaders.DbModel
{
    public class ColumnModel
    {
        public string Name { get; set; }

        public int? Position { get; set; }

        public string DefaultValue { get; set; }

        public string SqlDataType { get; set; }

        public int? Length { get; set; }

        public int? Precision { get; set; }

        public int? PrecisionRadix { get; set; }

        public int? Scale { get; set; }

        public int? DateTimePrecision { get; set; }

        public string SqlIsNullable { get; set; }
    }
}