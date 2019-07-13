using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class Solution : MetadataPart
    {
        private readonly Dictionary<string, string> _parameters;
        private readonly string _rootFolder;
        
        public Solution(string name, IEnumerable<ProjectionDeclaration> generators, IEnumerable<ShapeDeclaration> models, IEnumerable<PipelineDeclaration> pipelines, string rootFolder)
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
            parameters.TryGetValue("project-root", out string projectSolutionDirectory);

            if (!parameters.TryGetValue("project", out string projectFilePath))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(projectSolutionDirectory))
            {
                projectFilePath = Path.Combine(projectSolutionDirectory, projectFilePath);
            }

            if (!File.Exists(projectFilePath))
            {
                throw  new FileNotFoundException("It was not possible to find the shapeflow project file.", projectFilePath);
            }

            var result = ParseFile(projectFilePath, projectSolutionDirectory);
            result.AddParameters(parameters);
            return result;

        }

        public static Solution ParseFile(string path, string rootFolder = "")
        {
            return Parse(JObject.Parse(File.ReadAllText(path)), string.IsNullOrWhiteSpace(rootFolder) ? Path.GetDirectoryName(path) : rootFolder);
        }

        public static Solution Parse(JObject root, string rootFolder = "")
        {
            var name = root.GetStringPropertyValue("name");

            if(string.IsNullOrWhiteSpace(rootFolder))
            {
                rootFolder = Environment.CurrentDirectory;
            }
                        
            var generators = new List<ProjectionDeclaration>();
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
                    generators.Add(decl);
                }                
                else
                {
                    // maybe warn or throw?
                }
            }

            var shapesArray = root.GetValue("shapes") as JArray;
            foreach (var jToken in shapesArray ?? new JArray())
            {
                if (!(jToken is JObject shapeObject))
                {
                    continue;
                }

                var modelDeclaration = ShapeDeclaration.Parse(result, shapeObject);
                models.Add(modelDeclaration);
            }

            var pipelinesArray = root.GetValue("pipelines") as JArray;
            foreach (var jToken in pipelinesArray ?? new JArray())
            {
                if (!(jToken is JObject pipelineObject))
                {
                    continue;
                }

                var pipeline = PipelineDeclaration.Parse(pipelineObject);
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
