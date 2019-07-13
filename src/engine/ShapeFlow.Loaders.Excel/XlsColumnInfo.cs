using System;

namespace ShapeFlow.Loaders.Excel
{
    public class XlsColumnInfo
    {
        public string NameOnWorksheet { get; set; }

        public string NameOnDatabase { get; set; }

        public string NameOnCSharp { get; set; }

        public string LengthOnDatabase { get; set; }

        public string TypeOnDatabase { get; set; }

        public string Nullability { get; set; }

        public Type ImportAs { get; set; }

        public string CSharpType { get; set; }

        public string IsNullDefaultOnDatabase { get; set; }
    }
}