using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ShapeFlow.Infrastructure;
using ShapeFlow;
using ShapeFlow.Declaration;
using ShapeFlow.Loaders;
using ShapeFlow.Shapes;
using YamlDotNet.RepresentationModel;

namespace ShapeFlow.Loaders.Yaml
{
    public class YamlModelLoader : ILoader
    {
        public const string ModelPathParameter = "model-path";

        public YamlModelLoader()
        {            
        }

        public string Name => "YamlLoader";

        public ShapeFormat Format => throw new NotImplementedException();

        public ShapeContext Load(ShapeDeclaration context)
        {
            var modelFilePath = context.GetParameter(ModelPathParameter);

            using (var reader = new StreamReader(modelFilePath))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                var model = yamlStream.Documents.FirstOrDefault()?.RootNode;

                var modelContext = new ShapeContext(context, new YamlModel(model, ShapeFormat.Yaml, context.ModelName, context.Tags));

                return modelContext;
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

    public class YamlModel : Shape
    {
        public YamlModel(YamlNode root, ShapeFormat format, string name, IEnumerable<string> tags) : base(format, name, tags)
        {
            Root = root;
        }

        public YamlNode Root { get; }

        public override object GetInstance()
        {
            return Root;
        }
    }
}
