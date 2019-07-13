using System;
using System.IO;
using System.Linq;
using DotNetFileUtils;
using ShapeFlow.Declaration;

namespace ShapeFlow.Projections
{
    public class TextTemplateProvider
    {
        public TextTemplate GetFile(PipelineContext context, ProjectionDeclaration projection, ProjectionRuleDeclaration file)
        {
            return file.IsEmbedded ? TextTemplate.Create(file, GetResourceStream(file.TemplateName)) 
                : TextTemplate.Create(file, ResolveRulePath(context, projection, file));
        }

        protected string GetResourceText(string resourceName)
        {
            string result;
            
            var resourceStream = GetResourceStream(resourceName);
            if (resourceStream == null)
            {
                return null;
            }

            using (var reader = new StreamReader(resourceStream))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        protected Stream GetResourceStream(string resourceName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Where(a => a.GetManifestResourceNames().Length > 0 && a.GetManifestResourceNames().Any(s => s.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            if (assemblies.Length > 1)
            {
                throw new InvalidOperationException("Ambigous resource name");
            }
            else if (assemblies.Length == 0)
            {
                throw new InvalidOperationException("Resource not found");
            }

            var assembly = assemblies[0];
            var fullResourceName = assembly
                            .GetManifestResourceNames()
                            .First(s => s.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));

            // see if we can read it
            var resourceStream = assembly.GetManifestResourceStream(fullResourceName);
            return resourceStream;
        }

        protected string GetFileText(string fullPath)
        {
            string text = null;
                        
            if (File.Exists(fullPath))
            {
                text = File.ReadAllText(fullPath);
            }

            return text;
        }

        protected Stream GetFile(string fullPath)
        {
            Stream result = null;

            if (File.Exists(fullPath))
            {
                result = File.Open(fullPath, FileMode.Open);
            }

            return result;
        }

        protected string ResolveRulePath(PipelineContext context, ProjectionDeclaration projection, ProjectionRuleDeclaration rule)
        {
            var projectionDeploymentDirectory = new DirectoryPath(projection.Location);
            var filePath = projectionDeploymentDirectory.CombineWithFilePath(rule.TemplateName);
            return filePath.FullPath;
        }        
    }
}
