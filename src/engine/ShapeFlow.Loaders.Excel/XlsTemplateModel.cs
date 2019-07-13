using System;
using System.Collections.Generic;
using System.Linq;
using ShapeFlow.Shapes;

namespace ShapeFlow.Loaders.Excel
{
    public class XlsTemplateModelRoot
    {
        public string Namespace { get; set; }

        public string LineObjectName { get; set; }

        public string ObjectName { get; set; }

        public string MatchColumnNameOnFile { get; set; }

        public string SchemaName { get; set; }

        public IEnumerable<XlsColumnInfo> ColumnsToImport { get; set; }

        public IEnumerable<XlsColumnInfo> ColumnsToUpdate
        {
            get
            {
                return ColumnsToImport?
                    .Where(c => !c.NameOnDatabase.Equals(MatchColumnName))
                    .ToArray() ?? Enumerable.Empty<XlsColumnInfo>();
            }
        }

        public string MatchColumnName { get; set; }                               
    }
}