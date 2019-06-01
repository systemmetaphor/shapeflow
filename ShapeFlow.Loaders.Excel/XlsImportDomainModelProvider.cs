using System;
using System.IO;

namespace ShapeFlow.Loaders.Excel
{

    public class XlsImportDomainModelProvider : IModelLoader
    {
        private const string FileNameParameter = "fileName";
        private const string ObjectNameParameter = "objectName";
        private const string LineObjectNameParameter = "lineObjectName";
        private const string MatchColumnParameter = "matchColumn";
        private const string NamespaceParameter = "namespace";
        private const string SchemaParameter = "schema";

        public XlsImportDomainModelProvider()
        {
            DomainModelType = ExcelImporterTargetHandler.DomainModelName;
            Order = 0;
        }

        public string DomainModelType
        {
            get;
        }

        public int Order
        {
            get;
        }

        public DomainModelRoot LoadModel(TargetContext context)
        {
            var fileName = context.GetParameter(FileNameParameter);
            fileName = fileName.Replace("{{__dirname}}", Environment.CurrentDirectory);

            var outputDirectory = context.GetParameter("outputDirectory");
            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                outputDirectory = outputDirectory.Replace("{{__dirname}}", Environment.CurrentDirectory);
                context.GeneratorContext.OverrideOutputDirectory(outputDirectory);
            }
            
            var objectName = context.GetParameter(ObjectNameParameter);
            var lineObjectName = context.GetParameter(LineObjectNameParameter);
            var matchColumn = XlsImportGenerators.DatabaseFriendlyName(context.GetParameter(MatchColumnParameter));
            var ns = context.GetParameter(NamespaceParameter);
            var schemaName = context.GetParameter(SchemaParameter);

            var linesToSkip = 0;
            int.TryParse(context.GetParameter("LinesToSkip") ?? "0", out linesToSkip);

            var columnsModel = new XlsImportGenerators().GetColumnInfo(fileName, linesToSkip);
            return new XlsTemplateModel
            {
                LineObjectName = lineObjectName,
                ObjectName = objectName,
                ColumnsToImport = columnsModel,
                NamespaceOnCSharp = ns,
                MatchColumnNameOnFile = context.GetParameter(MatchColumnParameter),
                MatchColumnName = matchColumn,
                SchemaName = schemaName
            };
        }

        public bool ValidateArguments(TargetContext context, TextWriter output)
        {
            var isValid = true;

            if (string.IsNullOrWhiteSpace(context.GetParameter(FileNameParameter)))
            {
                isValid = false;
                output.WriteLine($"The parameter {FileNameParameter} is required.");
            }

            if (string.IsNullOrWhiteSpace(context.GetParameter(ObjectNameParameter)))
            {
                isValid = false;
                output.WriteLine($"The parameter {ObjectNameParameter} is required.");
            }

            if (string.IsNullOrWhiteSpace(context.GetParameter(LineObjectNameParameter)))
            {
                isValid = false;
                output.WriteLine($"The parameter {LineObjectNameParameter} is required.");
            }

            return isValid;
        }
    }
}
