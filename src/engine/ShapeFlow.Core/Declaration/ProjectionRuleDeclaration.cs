using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.Projections;

namespace ShapeFlow.Declaration
{
    public class ProjectionRuleDeclaration
    {
        public ProjectionRuleDeclaration(string fileName)
            : this(fileName, TextTemplateLanguages.DotLiquid)

        {
        }

        public ProjectionRuleDeclaration(string fileName, string language)
        {            
            FileName = fileName;
            Language = language;
            Parameters = new Dictionary<string, string>();            
        }
                        
        public string FileName
        {
            get;            
        }

        public string Language
        {
            get;
        }
        
        public IDictionary<string,string> Parameters { get; }

        public static ProjectionRuleDeclaration Parse(JObject ruleObject)
        {
            var fileName = ruleObject.GetStringPropertyValue("fileName");
            var ruleDeclaration = new ProjectionRuleDeclaration(fileName);
            return ruleDeclaration;
        }
    }
}
