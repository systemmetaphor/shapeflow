using System;
using System.Collections.Generic;
using System.Linq;

namespace ShapeFlow.Loaders.Excel
{
    public class XlsTemplateModel : DomainModelRoot
    {
        public string NamespaceOnCSharp { get; set; }

        public string LineObjectName { get; set; }

        public string ObjectName { get; set; }

        public string MatchColumnNameOnFile { get; set; }

        public string SchemaName { get; set; }

        public IEnumerable<XlsColumnInfo> ColumnsToImport { get; set; }

        public IEnumerable<XlsColumnInfo> ColumnsToUpdate
        {
            get
            {
                return ColumnsToImport?.Where(c => !c.NameOnDatabase.Equals(MatchColumnName)).ToArray() ?? Enumerable.Empty<XlsColumnInfo>();
            }
        }

        public string MatchColumnName { get; set; }

        public override void MergeWith(DomainModelRoot second)
        {
            throw new NotSupportedException();
        }

        public override void RegisterDomainTypes(Action<Type, string[]> register)
        {
            register(typeof(XlsColumnInfo), typeof(XlsColumnInfo).GetProperties().Select(p => p.Name).ToArray());
        }
    }
}