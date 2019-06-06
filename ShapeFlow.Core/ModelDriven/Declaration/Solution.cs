using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShapeFlow.Infrastructure;
using Newtonsoft.Json.Linq;

namespace ShapeFlow
{
    public class Solution
    {
        private Dictionary<string, string> _parameters;
        
        public Solution(string name, IEnumerable<LoaderDeclaration> loaders, IEnumerable<GeneratorRefDeclaration> generators, IEnumerable<ShapeDeclaration> models, IEnumerable<PipelineDeclaration> pipelines)
        {
            Name = name;
            Loaders = loaders;
            Generators = generators;
            Models = models;
            Pipelines = pipelines;
            _parameters = new Dictionary<string, string>();
        }

        public string Name { get; }

        public IEnumerable<LoaderDeclaration> Loaders { get; }

        public IEnumerable<GeneratorRefDeclaration> Generators { get; }

        public IEnumerable<ShapeDeclaration> Models { get; }

        public IEnumerable<PipelineDeclaration> Pipelines { get; }

        public IDictionary<string, string> Parameters => _parameters;

        public void AddParameters(IDictionary<string, string> parameters)
        {
            foreach(var parameter in parameters)
            {
                if(_parameters.ContainsKey(parameter.Key))
                {
                    _parameters[parameter.Key] = parameter.Value;
                }
                else
                {
                    _parameters.Add(parameter.Key, parameter.Value);
                }
            }
        }

        public static Solution ParseFile(string path)
        {
            return Parse(JObject.Parse(File.ReadAllText(path)));
        }

        public static Solution Parse(JObject root)
        {
            var name = root.GetStringPropertyValue("name");

            var loaders = new List<LoaderDeclaration>();
            var generators = new List<GeneratorRefDeclaration>();
            var models = new List<ShapeDeclaration>();
            var pipelines = new List<PipelineDeclaration>();

            var result = new Solution(name, loaders, generators, models, pipelines);

            var loadersObject = root.GetValue("loaders") as JObject;
            foreach (JProperty property in loadersObject?.Properties() ?? Enumerable.Empty<JProperty>())
            {
                var packageName = property.Name;
                var packageVersion = property.Value.Value<string>();
                loaders.Add(new LoaderDeclaration(packageName, packageVersion));
            }

            var generatorsObject = root.GetValue("generators") as JObject;
            foreach (JProperty property in generatorsObject?.Properties() ?? Enumerable.Empty<JProperty>())
            {
                var packageName = property.Name;
                if (property.Value.Type == JTokenType.Object)
                {
                    var declObject = property.Value as JObject;
                    var decl = GeneratorDeclaration.Parse(declObject, packageName);
                    generators.Add(new GeneratorRefDeclaration(packageName, decl.Version, decl));
                }
                else if (property.Value.Type == JTokenType.String)
                {
                    var packageVersion = property.Value.Value<string>();
                    generators.Add(new GeneratorRefDeclaration(packageName, packageVersion));
                }
                else
                {
                    // maybe warn or throw?
                }
            }

            var shapesArray = root.GetValue("shapes") as JArray;
            foreach (JObject shapeObject in shapesArray ?? new JArray())
            {
                ShapeDeclaration modelDeclaration = ShapeDeclaration.Parse(result, shapeObject);
                models.Add(modelDeclaration);
            }

            var pipelinesArray = root.GetValue("pipelines") as JArray;
            foreach (JObject pipelineObject in pipelinesArray ?? new JArray())
            {
                PipelineDeclaration pipeline = PipelineDeclaration.Parse(pipelineObject);
                pipelines.Add(pipeline);
            }

            
            return result;
        }

        public string GetParameter(string name)
        {
            if (_parameters?.ContainsKey(name) ?? false)
            {
                return _parameters[name];
            }

            return null;
        }
    }
}
