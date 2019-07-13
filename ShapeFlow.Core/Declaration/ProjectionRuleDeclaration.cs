using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.Projections;

namespace ShapeFlow.Declaration
{
    public class ProjectionRuleDeclaration
    {
        public ProjectionRuleDeclaration(string templateName)
            : this(templateName, false, null)
        {
        }

        public ProjectionRuleDeclaration(string templateName, bool isEmbeddedTemplate, string outputPathTemplate)
            : this(templateName, TextTemplateLanguages.DotLiquid, isEmbeddedTemplate, outputPathTemplate)

        {
        }

        public ProjectionRuleDeclaration(string templateName, string templateLanguage, bool isEmbeddedTemplate, string outputPathTemplate)
        {            
            TemplateName = templateName;
            TemplateLanguage = templateLanguage;
            OutputPathTemplate = outputPathTemplate ?? string.Empty;
            IsEmbedded = isEmbeddedTemplate;
            Parameters = new Dictionary<string, string>();            
        }
                        
        public string TemplateName
        {
            get;            
        }

        public string TemplateLanguage
        {
            get;
        }

        public string OutputPathTemplate
        {
            get;            
        }
        
        public bool IsEmbedded
        {
            get;            
        }

        public IDictionary<string,string> Parameters { get; }

        public static ProjectionRuleDeclaration Parse(JObject ruleObject)
        {
            var templateName = ruleObject.GetStringPropertyValue("templateName");
            var outputPathTemplate = ruleObject.GetStringPropertyValue("outputPathTemplate");
            var isEmbedded = ruleObject.GetValue("isEmbedded")?.Value<bool>() ?? false;
            var ruleDeclaration = new ProjectionRuleDeclaration(templateName, isEmbedded, outputPathTemplate);
            return ruleDeclaration;
        }
    }
}
