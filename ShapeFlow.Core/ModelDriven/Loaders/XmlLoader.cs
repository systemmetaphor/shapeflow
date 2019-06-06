using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.Shapes;

namespace ShapeFlow.Loaders
{
    public class XmlLoader : ILoader
    {
        public const string ModelPathParameter = "model-path";

        public XmlLoader()
        {            
        }

        public string Name => "XmlLoader";

        public ShapeFormat Format => ShapeFormat.Xml;

        public ShapeContext Load(ShapeDeclaration context)
        {
            var modelFilePath = context.GetParameter(ModelPathParameter);

            using (var file = File.OpenRead(modelFilePath))
            {
                var document = XDocument.Load(file);
                return new ShapeContext(context, new XmlShape(document, ShapeFormat.Xml, context.ModelName, context.Tags));
            }
        }

        public bool ValidateArguments(ShapeDeclaration context)
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
}
