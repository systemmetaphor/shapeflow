using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.Models;

namespace ShapeFlow.Loaders
{
    public class XmlModelLoader : IModelLoader
    {
        public const string ModelPathParameter = "model-path";

        public XmlModelLoader()
        {            
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
                AppTrace.Error($"The parameter {ModelPathParameter} is required.");
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
