using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetFileUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.PackageManagement;

namespace ShapeFlow.Declaration
{
    /// <summary>
    /// A generator converts an input model using a set of projectionRef rules into a set of output files.     
    /// This class holds the configuration required to set a new projectionRef.
    /// </summary>
    /// <remarks>
    /// The outputs represent what is expected on the end of the projectionRef process.
    /// </remarks>
    public class ProjectionDeclaration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionDeclaration"/> class.
        /// </summary>
        private ProjectionDeclaration()
        {
        }

        /// <summary>
        /// Gets an user friendly name for the projectionRef.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the location of the projectionRef.
        /// </summary>
        /// <value>
        /// The location of the projectionRef..
        /// </value>
        /// <remarks>
        /// This value will be used to locate the projectionRef templates and if applicable other projectionRef dependencies.
        /// </remarks>
        public string Location
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the version of the projection.
        /// </summary>
        public string Version
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the package name.
        /// </summary>
        public string PackageId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a flag indicating if the referenced projection is declared inline on the project file.
        /// </summary>
        public bool IsInline => string.IsNullOrWhiteSpace(PackageId);

        /// <summary>
        /// Gets the base path for all rules.
        /// </summary>
        public string RulesBasePath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the projection expression.
        /// </summary>
        public string ProjectionExpression
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the rules used to project the model into the desired output.
        /// </summary>
        /// <value>
        /// The rules used to project the model into the desired output..
        /// </value>
        public IEnumerable<ProjectionRuleDeclaration> Rules
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the parameters required by the projection.
        /// </summary>
        public IEnumerable<ParameterDeclaration> Parameters
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets metadata about the output produced by the projection.
        /// </summary>
        public OutputDeclaration Output
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets metadata about the input expected by the projection.
        /// </summary>
        public InputDeclaration Input
        {
            get;
            private set;
        }

        public ProjectionDeclaration AddRule(ProjectionRuleDeclaration rule)
        {
            var copy = Clone();
            var rules = new List<ProjectionRuleDeclaration>(copy.Rules) { rule }; // old set + rule
            copy.Rules = rules;
            return copy;
        }

        public static ProjectionDeclaration Create(
            string name, 
            string location, 
            string rulesBasePath, 
            IEnumerable<ProjectionRuleDeclaration> rules, 
            InputDeclaration input, 
            OutputDeclaration output)
        {
            return new ProjectionDeclaration
            {
                Name = name,
                Location =  location,
                RulesBasePath = rulesBasePath,
                Rules =  rules,
                Input =  input,
                Output =  output
            };
        }

        /// <summary>
        /// Loads the package metadata from the configuration file (shapeflow.package.json)
        /// or infers it based on the contents of the nuget package Content folder.
        /// </summary>
        /// <param name="existingDeclaration"></param>
        /// <param name="package"></param>
        /// <param name="searchExpressions"></param>
        /// <remarks>
        /// Because we don't know what kind of templates to expect we depend on the
        /// caller to pass list of globs (searchExpressions). In the normal flow this
        /// is done by reading the SearchExpression property on each template engine.
        /// </remarks>
        /// <returns></returns>
        public static ProjectionDeclaration LoadOrInferMetadata(
            ProjectionDeclaration existingDeclaration,
            PackageInfo package,
            IEnumerable<string> searchExpressions)
        {
            var rules = new List<string>();

            var directoryPath = new DirectoryPath(package.Root);
            var contentPath = directoryPath.Combine("Content");
            var metadataFile = contentPath.CombineWithFilePath(new FilePath("shapeflow.package.json"));
            if (File.Exists(metadataFile.FullPath))
            {
                return FromFile(metadataFile.FullPath);
            }

            // use conventions to derive the metadata

            var globber = new Globber();
            foreach (var searchExpression in searchExpressions)
            {
                var fullGlobPattern = contentPath.Combine(searchExpression);
                var templates = globber.Match(fullGlobPattern.FullPath);
                templates = templates
                    .OfType<FilePath>()
                    .Select(contentPath.GetRelativePath)
                    .ToArray();

                rules.AddRange(
                    templates
                    .Select(template => template.FullPath)
                    .ToList());
            }

            var parameters = new List<ParameterDeclaration>();
            existingDeclaration.Parameters = parameters;
            existingDeclaration.Rules = rules.Distinct().Select(s => new ProjectionRuleDeclaration(s)).ToArray();
            existingDeclaration.Location = contentPath.FullPath;

            return existingDeclaration;
        }

        /// <summary>
        /// Reads the projection metadata from the given file.
        /// </summary>
        /// <param name="path">The file.</param>
        /// <returns>The projection metadata.</returns>
        public static ProjectionDeclaration FromFile(string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Environment.CurrentDirectory;
            }

            var text = File.ReadAllText(path);
            var jo = JObject.Parse(text);
            if (!jo.ContainsKey("location"))
            {
                jo.Add("location", directory);
            }

            var result = Parse(jo);
            return result;
        }

        /// <summary>
        /// Reads the projection metadata from the given json object.
        /// </summary>
        /// <param name="declaration">The json object.</param>
        /// <param name="name">The name of the projectionRef.</param>
        /// <returns>The projection metadata.</returns>
        public static ProjectionDeclaration Parse(JObject declaration, string name = null)
        {
            name = name ?? declaration.GetStringPropertyValue("name");
            var version = declaration.GetStringPropertyValue("version");
            var projectionExpression = declaration.GetStringPropertyValue("projectionExpression", "map(i,f,o)");
            var parameters = declaration.ParseParameters("parameters");

            var rulesArray = declaration.GetValue("rules") as JArray ?? new JArray();
            var rules = new List<ProjectionRuleDeclaration>();
            foreach (var jToken in rulesArray)
            {
                var ruleObject = (JObject)jToken;
                var ruleDeclaration = ProjectionRuleDeclaration.Parse(ruleObject);
                rules.Add(ruleDeclaration);
            }

            var location = declaration.GetStringPropertyValue("location");
            var rulesBasePath = declaration.GetStringPropertyValue("rulesBasePath");

            var inputObject = declaration.GetValue("input") as JObject;
            var outputObject = declaration.GetValue("output") as JObject;
            if (outputObject == null)
            {
                throw new InvalidOperationException();
            }

            var output = OutputDeclaration.Parse(outputObject);

            if (inputObject == null)
            {
                throw new InvalidOperationException();
            }

            var input = InputDeclaration.Parse(inputObject);

            var projection = new ProjectionDeclaration
            {
                Name = name,
                Parameters = parameters,
                Rules = rules,
                Location = location,
                Version = version,
                RulesBasePath = rulesBasePath,
                ProjectionExpression = projectionExpression,
                Input = input,
                Output = output
            };

            string packageName = null;

            if (declaration.ContainsKey("packageId"))
            {
                packageName = declaration.GetStringPropertyValue("packageId");
            }

            projection.PackageId = packageName;

            return projection;
        }

        public static void WriteTo(JsonTextWriter writer, ProjectionDeclaration value)
        {
            writer.WriteStartObject();
            
            writer.WritePropertyName(nameof(Location).ToCamelCase());
            writer.WriteValue(value.Location);

            writer.WritePropertyName(nameof(Version).ToCamelCase());
            writer.WriteValue(value.Version);

            writer.WritePropertyName(nameof(PackageId).ToCamelCase());
            writer.WriteValue(value.PackageId);

            writer.WritePropertyName(nameof(RulesBasePath).ToCamelCase());
            writer.WriteValue(value.RulesBasePath);

            writer.WritePropertyName(nameof(ProjectionExpression).ToCamelCase());
            writer.WriteValue(value.ProjectionExpression);

            writer.WritePropertyName(nameof(Rules).ToCamelCase());

            writer.WriteStartArray();

            foreach (var rule in value.Rules)
            {
                ProjectionRuleDeclaration.WriteTo(writer, rule);
            }

            writer.WriteEndArray();

            writer.WritePropertyName(nameof(Input).ToCamelCase());
            InputDeclaration.WriteTo(writer, value.Input);

            writer.WritePropertyName(nameof(Output).ToCamelCase());
            OutputDeclaration.WriteTo(writer, value.Output);

            writer.WriteEndObject();
        }

        private ProjectionDeclaration Clone()
        {
            return new ProjectionDeclaration
            {
                Name = Name,
                Input = Input,
                Location = Location,
                Output = Output,
                Version = Version,
                PackageId = PackageId,
                Parameters = new List<ParameterDeclaration>(Parameters),
                Rules = new List<ProjectionRuleDeclaration>(Rules),
                RulesBasePath = RulesBasePath,
                ProjectionExpression = ProjectionExpression,

            };
        }
    }
}
