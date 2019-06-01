using System;
using System.IO;
using System.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow;

namespace ShapeFlow
{
    public class TextTemplateProvider
    {
        public TextTemplateProvider()
        {
        }

        public TextTemplate GetFile(ProjectionContext context, TransformationRuleDeclaration file)
        {
            if(file.IsEmbedded)
            {
                return TextTemplate.Create(file, GetResourceStream(file.TemplateName));
            }
            else
            {
                return TextTemplate.Create(file, ResolveRulePath(context, file));
            }
        }

        protected string GetResourceText(string resourceName)
        {
            string result = null;
            var resourceStream = GetResourceStream(resourceName);
            if (resourceStream != null)
            {
                using (var reader = new StreamReader(resourceStream))
                {
                    result = reader.ReadToEnd();
                }
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

        protected string ResolveRulePath(ProjectionContext context, TransformationRuleDeclaration rule)
        {            
            var requestedFile = rule.TemplateName;
            return requestedFile;
        }        
    }
}
