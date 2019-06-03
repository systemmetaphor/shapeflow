using System;
using System.Collections.Generic;
using System.IO;
using ShapeFlow.Infrastructure;
using ShapeFlow.Models;

namespace ShapeFlow.Loaders.Excel
{

    public class ExcelModelLoader: IModelLoader
    {
        private const string FileNameParameter = "fileName";
        private const string ObjectNameParameter = "objectName";
        private const string LineObjectNameParameter = "lineObjectName";
        private const string MatchColumnParameter = "matchColumn";
        private const string NamespaceParameter = "namespace";
        private const string SchemaParameter = "schema";

        public ExcelModelLoader()
        {
        }

        public string Name
        {
            get
            {
                return "Excel";
            }
        }

        public ModelFormat Format => ModelFormat.Clr;

        public ModelContext LoadModel(ModelDeclaration context)
        {
            var fileName = context.GetParameter(FileNameParameter);
            fileName = fileName.Replace("{{__dirname}}", Environment.CurrentDirectory);

            var outputDirectory = context.GetParameter("outputDirectory");
            var objectName = context.GetParameter(ObjectNameParameter);
            var lineObjectName = context.GetParameter(LineObjectNameParameter);
            var matchColumn = XlsImportGenerators.DatabaseFriendlyName(context.GetParameter(MatchColumnParameter));
            var ns = context.GetParameter(NamespaceParameter);
            var schemaName = context.GetParameter(SchemaParameter);
ExcelModelLoad
            int.TryParse(context.GetParameter("LinesToSkip") ?? "0", out int linesToSkip);

            var columnsModel = new XlsImportGenerators().GetColumnInfo(fileName, linesToSkip);

            return new ModelContext(context, new XlsTemplateModel(
                new XlsTemplateModelRoot
                {
                    LineObjectName = lineObjectName,
                    ObjectName = objectName,
                    ColumnsToImport = columnsModel,
                    NamespaceOnCSharp = ns,
                    MatchColumnNameOnFile = context.GetParameter(MatchColumnParameter),
                    MatchColumnName = matchColumn,
                    SchemaName = schemaName
                }, ModelFormat.Clr, context.ModelName, context.Tags));
        }

        public bool ValidateArguments(ModelDeclaration context)
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
            }ExcelModelLoad

            if (string.IsNullOrWhiteSpace(context.GetParameter(LineObjectNameParameter)))
            {ExcelModelLoad
                isValid = false;
                AppTrace.Error($"The parameter {LineObjectNameParameter} is required.");
            }

            return isValid;
        }
    }

    public class XlsTemplateModel : Model
    {
        public XlsTemplateModel(XlsTemplateModelRoot root, ModelFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = root;
        }

        public XlsTemplateModelRoot Root { get; }

        public override object GetModelInstance()
        {
            return Root;
        }
    }
}
