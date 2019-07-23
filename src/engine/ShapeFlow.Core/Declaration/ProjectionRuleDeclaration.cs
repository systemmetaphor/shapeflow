using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.Projections;

namespace ShapeFlow.Declaration
{
    public class ProjectionRuleDeclaration
    {
        private ProjectionRuleDeclaration()
        {

        }

        public ProjectionRuleDeclaration(string fileName)
            : this(fileName, RuleLanguages.DotLiquid)

        {
        }

        public ProjectionRuleDeclaration(string fileName, string language)
        {            
            FileName = fileName;
            Language = language;
            Parameters =Enumerable.Empty<ParameterDeclaration>();
        }
                        
        public string FileName
        {
            get;
            private set;
        }

        public string Language
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
        /// Gets the parameters required by the projection rule.
        /// </summary>
        public IEnumerable<ParameterDeclaration> Parameters
        {
            get;
            private set;
        }

        public static ProjectionRuleDeclaration Parse(JObject ruleObject)
        {
            var fileName = ruleObject.GetStringPropertyValue("fileName");
            var projectionExpression = ruleObject.GetStringPropertyValue("projectionExpression");
            var parameters = ruleObject.ParseParameters("parameters");
            var language = ruleObject.GetStringPropertyValue("language", RuleLanguages.DotLiquid);

            var ruleDeclaration = new ProjectionRuleDeclaration
            {
                FileName = fileName,
                Parameters = parameters,
                ProjectionExpression = projectionExpression,
                Language = language
            };

            return ruleDeclaration;
        }
    }
}
