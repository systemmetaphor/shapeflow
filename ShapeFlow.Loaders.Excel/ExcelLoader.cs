using System;
using System.Collections.Generic;
using System.IO;
using ShapeFlow.Infrastructure;
using ShapeFlow.Shapes;

namespace ShapeFlow.Loaders.Excel
{

    public class ExcelLoader: ILoader
    {
        private const string FileNameParameter = "fileName";
        private const string ObjectNameParameter = "objectName";
        private const string LineObjectNameParameter = "lineObjectName";
        private const string MatchColumnParameter = "matchColumn";
        private const string NamespaceParameter = "namespace";
        private const string SchemaParameter = "schema";

        public ExcelLoader()
        {
        }

        public string Name
        {
            get
            {
                return "Excel";
            }
        }

        public ShapeFormat Format => ShapeFormat.Clr;

        public ShapeContext Load(ShapeDeclaration context)
        {
            var fileName = context.GetParameter(FileNameParameter);
            fileName = fileName.Replace("{{__dirname}}", Environment.CurrentDirectory);

            var outputDirectory = context.GetParameter("outputDirectory");
            var objectName = context.GetParameter(ObjectNameParameter);
            var lineObjectName = context.GetParameter(LineObjectNameParameter);
            var matchColumn = XlsImportGenerators.DatabaseFriendlyName(context.GetParameter(MatchColumnParameter));
            var ns = context.GetParameter(NamespaceParameter);
            var schemaName = context.GetParameter(SchemaParameter);
            int.TryParse(context.GetParameter("LinesToSkip") ?? "0", out int linesToSkip);

            var columnsModel = new XlsImportGenerators().GetColumnInfo(fileName, linesToSkip);

            return new ShapeContext(context, new XlsTemplateModel(
                new XlsTemplateModelRoot
                {
                    LineObjectName = lineObjectName,
                    ObjectName = objectName,
                    ColumnsToImport = columnsModel,
                    NamespaceOnCSharp = ns,
                    MatchColumnNameOnFile = context.GetParameter(MatchColumnParameter),
                    MatchColumnName = matchColumn,
                    SchemaName = schemaName
                }, ShapeFormat.Clr, context.ModelName, context.Tags));
        }

        public bool ValidateArguments(ShapeDeclaration context)
        {
            var isValid = true;

            if (string.IsNullOrWhiteSpace(context.GetParameter(FileNameParameter)))
            {
                isValid = false;
                AppTrace.Error($"The parameter {FileNameParameter} is required.");
            }

            if (string.IsNullOrWhiteSpace(context.GetParameter(ObjectNameParameter)))
            {
                isValid = false;
                AppTrace.Error($"The parameter {ObjectNameParameter} is required.");
            }

            if (string.IsNullOrWhiteSpace(context.GetParameter(LineObjectNameParameter)))
            {
                isValid = false;
                AppTrace.Error($"The parameter {LineObjectNameParameter} is required.");
            }

            return isValid;
        }
    }

    public class XlsTemplateModel : Shape
    {
        public XlsTemplateModel(XlsTemplateModelRoot root, ShapeFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = root;
        }

        public XlsTemplateModelRoot Root { get; }

        public override object GetInstance()
        {
            return Root;
        }
    }
}
