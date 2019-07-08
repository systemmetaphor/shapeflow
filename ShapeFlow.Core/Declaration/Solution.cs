using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class MetadataPart
    {
        public MetadataPart(string name, IEnumerable<ProjectionRefDeclaration> generators, IEnumerable<ShapeDeclaration> models, IEnumerable<PipelineDeclaration> pipelines)
        {
            Name = name;
            Projections = generators;
            Models = models;
            Pipelines = pipelines;
        }

        public string Name { get; }

        public IEnumerable<ProjectionRefDeclaration> Projections { get; }

        public IEnumerable<ShapeDeclaration> Models { get; }

        public IEnumerable<PipelineDeclaration> Pipelines { get; }
    }

    public class Solution : MetadataPart
    {
        private Dictionary<string, string> _parameters;
        private string _rootFolder;
        
        public Solution(string name, IEnumerable<ProjectionRefDeclaration> generators, IEnumerable<ShapeDeclaration> models, IEnumerable<PipelineDeclaration> pipelines, string rootFolder)
            : base(name, generators, models, pipelines)
        {
            _parameters = new Dictionary<string, string>();

            if(string.IsNullOrWhiteSpace(rootFolder))
            {
                throw new ArgumentNullException(nameof(rootFolder));
            }
            
            // I have opted not to assert if the root folder exists because it would tie the constructor to the Directory
            // class and would make it harder to move to a virtual filesystem design
            
            _rootFolder = rootFolder;
        }         

        public IDictionary<string, string> Parameters => _parameters;

        public string RootDirectory => _rootFolder;

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

        public static Solution ParseFile(IDictionary<string, string> parameters)
        {
            if(parameters.TryGetValue("project", out string projectPath))
            {
                var result = ParseFile(projectPath);
                result.AddParameters(parameters);
                return result;
            }

            return null;
        }

        public static Solution ParseFile(string path)
        {
            return Parse(JObject.Parse(File.ReadAllText(path)), Path.GetDirectoryName(path));
        }

        public static Solution Parse(JObject root, string rootFolder = "")
        {
            var name = root.GetStringPropertyValue("name");

            if(string.IsNullOrWhiteSpace(rootFolder))
            {
                rootFolder = Environment.CurrentDirectory;
            }
                        
            var generators = new List<ProjectionRefDeclaration>();
            var models = new List<ShapeDeclaration>();
            var pipelines = new List<PipelineDeclaration>();

            var result = new Solution(name, generators, models, pipelines, rootFolder);                       

            var generatorsObject = root.GetValue("projections") as JObject;
            foreach (JProperty property in generatorsObject?.Properties() ?? Enumerable.Empty<JProperty>())
            {
                
                if (property.Value.Type == JTokenType.Object)
                {
                    var declObject = property.Value as JObject;

                    var decl = ProjectionDeclaration.Parse(declObject, property.Name);
                    
                    generators.Add(new ProjectionRefDeclaration(decl));
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
                if(pipeline == null)
                {
                    continue;
                }

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
