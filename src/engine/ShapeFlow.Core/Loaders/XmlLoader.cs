using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ShapeFlow.Declaration;
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

        public Task<ShapeContext> Load(ShapeDeclaration context)
        {
            var modelFilePath = context.GetParameter(ModelPathParameter);

            using (var file = File.OpenRead(modelFilePath))
            {
                var document = XDocument.Load(file);
                return Task.FromResult(new ShapeContext(context, new XmlShape(document, ShapeFormat.Xml, context.ModelName, context.Tags)));
            }
        }

        public Task Save(ShapeContext context)
        {
            throw new NotImplementedException();
        }

        public ShapeContext Create(ShapeDeclaration decl)
        {
            throw new NotImplementedException();
        }

        public ShapeContext CreateSet(ShapeDeclaration decl)
        {
            throw new NotImplementedException();
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
