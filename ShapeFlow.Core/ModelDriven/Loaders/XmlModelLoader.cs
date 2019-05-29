using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.ModelDriven.Models;

namespace ShapeFlow.ModelDriven.Loaders
{
    public class XmlModelLoader : IModelLoader
    {
        private readonly ILoggingService _loggingService;

        public const string ModelPathParameter = "model-path";

        public XmlModelLoader(ILoggingService loggingService)
        {
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        public string Name => "XmlLoader";

        public ModelFormat Format => ModelFormat.Xml;

        public ModelContext LoadModel(ModelDeclaration context)
        {
            var modelFilePath = context.GetParameter(ModelPathParameter);

            using (var file = File.OpenRead(modelFilePath))
            {
                var document = XDocument.Load(file);
                return new ModelContext(context, new XmlModel(document, ModelFormat.Xml, context.ModelName, context.Tags));
            }
        }

        public bool ValidateArguments(ModelDeclaration context)
        {
            if (string.IsNullOrWhiteSpace(context.GetParameter(ModelPathParameter)))
            {
                // TODO: this should be a validation service
                _loggingService.Error($"The parameter {ModelPathParameter} is required.");
                return false;
            }

            return true;
        }
    }

    public class XmlModel : Model
    {
        public XmlModel(XDocument root,  ModelFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = root;
        }

        public XDocument Root { get; }

        public override object GetModelInstance()
        {
            return Root;
        }
    }
}
