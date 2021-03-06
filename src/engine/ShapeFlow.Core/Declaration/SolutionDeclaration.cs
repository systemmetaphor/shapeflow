﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShapeFlow.Collections;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class SolutionDeclaration : MetadataPart
    {
        private readonly Dictionary<string, string> _parameters;

        public SolutionDeclaration()
        {
            _parameters = new Dictionary<string, string>();
        }         

        public IDictionary<string, string> Parameters => _parameters;

        public string RootDirectory { get; private set; }

        public void AddParameters(IDictionary<string, string> parameters)
        {
            _parameters.AddOrUpdate(parameters);
        }

        public static SolutionDeclaration Create(
            string name, 
            string rootDirectory, 
            IEnumerable<ProjectionDeclaration> projections,
            IEnumerable<ShapeDeclaration> shapes,
            IEnumerable<PipelineDeclaration> pipelines)
        {
            return new SolutionDeclaration
            {
                Name = name,
                RootDirectory = rootDirectory,
                Projections =  projections,
                Shapes = shapes,
                Pipelines =  pipelines
            };
        }

        public static SolutionDeclaration ParseFile(IDictionary<string, string> parameters)
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

        public static SolutionDeclaration ParseFile(string path, string rootFolder = "")
        {
            return Parse(JObject.Parse(File.ReadAllText(path)), string.IsNullOrWhiteSpace(rootFolder) ? Path.GetDirectoryName(path) : rootFolder);
        }

        public static SolutionDeclaration Parse(string solutionJson, string rootFolder = "")
        {
            return Parse(JObject.Parse(solutionJson), rootFolder);
        }

        public static SolutionDeclaration Parse(JObject root, string rootFolder = "")
        {
            var name = root.GetStringPropertyValue("name");

            if(string.IsNullOrWhiteSpace(rootFolder))
            {
                rootFolder = Environment.CurrentDirectory;
            }
                        
            var projections = new List<ProjectionDeclaration>();
            var shapes = new List<ShapeDeclaration>();
            var pipelines = new List<PipelineDeclaration>();

            var result = new SolutionDeclaration
            {
                RootDirectory = rootFolder,
                Name = name,
                Pipelines = pipelines,
                Projections = projections,
                Shapes = shapes
            };


            if (!(root.GetValue("projections") is JObject projectionsObject))
            {
                throw new SolutionParsingException("The providers section is required.");
            }

            foreach (var property in projectionsObject.Properties())
            {
                if (property.Value.Type == JTokenType.Object)
                {
                    var declObject = property.Value as JObject;
                    var decl = ProjectionDeclaration.Parse(declObject, property.Name);
                    projections.Add(decl);
                }                
                else
                {
                    throw new SolutionParsingException($"Unexpected property '{property.Name}' value on the 'projections' section.");
                }
            }

            if (projections.Count == 0)
            {
                throw new SolutionParsingException("The provided file does not declare projections. At least one projection is required.");
            }

            if (!(root.GetValue("shapes") is JObject shapesObject))
            {
                throw new SolutionParsingException("The shapes section is required.");
            }

            foreach (var property in shapesObject.Properties())
            {
                if (property.Value.Type == JTokenType.Object)
                {
                    var shapeObject = (JObject)property.Value;
                    var modelDeclaration = ShapeDeclaration.Parse(shapeObject, property.Name);
                    shapes.Add(modelDeclaration);
                }
                else
                {
                    throw new SolutionParsingException($"Unexpected property '{property.Name}' value on the 'shapes' section.");
                }
            }

            if (shapes.Count == 0)
            {
                throw new SolutionParsingException("The provided file does not declare shapes. At least one shape is required.");
            }

            if (!(root.GetValue("pipelines") is JObject pipelinesDeclObject))
            {
                throw new SolutionParsingException("The pipelines section is required.");
            }

            foreach (var property in pipelinesDeclObject.Properties())
            {
                if (property.Value.Type == JTokenType.Object)
                {
                    var pipelineDeclObject = (JObject) property.Value;
                    var pipeline = PipelineDeclaration.Parse(pipelineDeclObject, property.Name, result);
                    pipelines.Add(pipeline);
                }
            }

            if (pipelines.Count == 0)
            {
                throw new SolutionParsingException("The provided file does not declare pipelines. At least one pipeline is required.");
            }

            return result;
        }


        public static string Save(SolutionDeclaration value)
        {
            var builder = new StringBuilder();
            var writer = new JsonTextWriter(new StringWriter(builder));
            writer.Formatting = Formatting.Indented;

            writer.WriteStartObject();

            writer.WritePropertyName(nameof(Name).ToCamelCase());
            writer.WriteValue(value.Name);

            writer.WritePropertyName(nameof(RootDirectory).ToCamelCase());
            writer.WriteValue(value.RootDirectory);

            writer.WritePropertyName(nameof(Shapes).ToCamelCase());
            writer.WriteStartObject();

            foreach (var shape in value.Shapes)
            {
                writer.WritePropertyName(shape.Name.ToCamelCase());
                ShapeDeclaration.WriteTo(writer, shape);
            }

            writer.WriteEndObject();


            writer.WritePropertyName(nameof(Projections).ToCamelCase());
            writer.WriteStartObject();

            foreach (var shape in value.Projections)
            {
                writer.WritePropertyName(shape.Name.ToCamelCase());
                ProjectionDeclaration.WriteTo(writer, shape);
            }

            writer.WriteEndObject();


            writer.WritePropertyName(nameof(Pipelines).ToCamelCase());
            writer.WriteStartObject();

            foreach (var pipeline in value.Pipelines)
            {
                writer.WritePropertyName(pipeline.Name.ToCamelCase());
                PipelineDeclaration.WriteTo(writer, pipeline);
            }

            writer.WriteEndObject();


            writer.WriteEndObject();
            
            writer.Close();
            return builder.ToString();
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
