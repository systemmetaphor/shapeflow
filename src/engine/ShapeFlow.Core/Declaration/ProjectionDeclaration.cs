using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetFileUtils;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.PackageManagement;
using Path = System.IO.Path;

namespace ShapeFlow.Declaration
{
    /// <summary>
    /// A generator converts an input model using a set of transformation rules into a set of output files.     
    /// This class holds the configuration required to set a new transformation.
    /// </summary>
    /// <remarks>
    /// The outputs represent what is expected on the end of the transformation process.
    /// </remarks>
    public class ProjectionDeclaration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransformationDeclaration"/> class.
        /// </summary>
        private  ProjectionDeclaration()
        {
        }

        /// <summary>
        /// Gets an user friendly name for the transformation.
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

        public IEnumerable<ParameterDeclaration> Parameters
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the location of the transformation.
        /// </summary>
        /// <value>
        /// The location of the transformation..
        /// </value>
        /// <remarks>
        /// This value will be used to locate the transformation templates and if applicable other transformation dependencies.
        /// </remarks>
        public string Location
        {
            get;
            private set;
        }

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
        /// Gets a flag indicating if the referenced projection is an inline one.
        /// </summary>
        public bool IsInline => string.IsNullOrWhiteSpace(PackageId);

        public static ProjectionDeclaration AppendPackageMetadata(ProjectionDeclaration existingDeclaration, PackageInfo package)
        {
            var directoryPath = new DirectoryPath(package.Root);
            var contentPath = directoryPath.Combine("Content");
            var metadataFile = contentPath.CombineWithFilePath(new FilePath("shapeflow.package.json"));
            if (File.Exists(metadataFile.FullPath))
            {
                return FromFile(metadataFile.FullPath);
            }

            // use conventions to derive the metadata
            var globber = new Globber();
            var fullGlobPattern = contentPath.Combine(".\\**\\*.liquid");
            var templates = globber.Match(fullGlobPattern.FullPath);
            templates = templates
                .OfType<FilePath>()
                .Select(contentPath.GetRelativePath)
                .ToArray();

            var parameters = new List<ParameterDeclaration>();

            var rules = templates
                .Select(template => new ProjectionRuleDeclaration(template.FullPath))
                .ToList();
                
            existingDeclaration.Parameters = parameters;
            existingDeclaration.Rules = rules;
            existingDeclaration.Location = contentPath.FullPath;
                
            return existingDeclaration;
        }

        public static ProjectionDeclaration FromFile(string path)
        {
            var directory = Path.GetDirectoryName(path);

            if(string.IsNullOrWhiteSpace(directory))
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

        public static ProjectionDeclaration Parse(JObject transformationObject, string transformationName = null)
        {
            

            // when its an inline decl it gets the name from the property holding the decl object
            transformationName = transformationName ?? transformationObject.GetStringPropertyValue("name");
            var version = transformationObject.GetStringPropertyValue("version");
            var parametersArray = transformationObject.GetValue("parameters") as JArray ?? new JArray();
            var parameters = new List<ParameterDeclaration>();
            foreach (var jToken in parametersArray)
            {
                var parametersObject = (JObject) jToken;
                var parameterDeclaration = ParameterDeclaration.Parse(parametersObject);
                parameters.Add(parameterDeclaration);
            }

            var rulesArray = transformationObject.GetValue("rules") as JArray ?? new JArray();
            var rules = new List<ProjectionRuleDeclaration>();
            foreach (var jToken in rulesArray)
            {
                var ruleObject = (JObject) jToken;
                var ruleDeclaration = ProjectionRuleDeclaration.Parse(ruleObject);
                rules.Add(ruleDeclaration);
            }

            var location = transformationObject.GetStringPropertyValue("location");

            var transformation = new ProjectionDeclaration
            {
                Name = transformationName,
                Parameters = parameters,
                Rules = rules,
                Location = location,
                Version = version
            };

            string packageName = null;

            if (transformationObject.ContainsKey("packageId"))
            {
                packageName = transformationObject.GetStringPropertyValue("packageId");
            }

            transformation.PackageId = packageName;

            return transformation;
        }
    }
}
