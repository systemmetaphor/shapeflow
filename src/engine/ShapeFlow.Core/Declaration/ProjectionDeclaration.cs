using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetFileUtils;
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
        /// Initializes a new instance of the <see cref="ProjectionRefDeclaration"/> class.
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

        public string ProjectionExpression
        {
            get;
            private set;
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
                directory = System.Environment.CurrentDirectory;
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

            var projection = new ProjectionDeclaration
            {
                Name = name,
                Parameters = parameters,
                Rules = rules,
                Location = location,
                Version = version,
                RulesBasePath =  rulesBasePath,
                ProjectionExpression = projectionExpression
            };

            string packageName = null;

            if (declaration.ContainsKey("packageId"))
            {
                packageName = declaration.GetStringPropertyValue("packageId");
            }

            projection.PackageId = packageName;

            return projection;
        }
    }
}
