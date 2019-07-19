using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using ShapeFlow.Declaration;
using ShapeFlow.Projections;
using ShapeFlow.Shapes;

namespace ShapeFlow.RuleEngines.Roslyn
{
    public class RoslynRuleEngine : IProjectionRuleEngine
    {
        private readonly ProjectionRuleProvider _fileProvider;

        public RoslynRuleEngine(ProjectionRuleProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public string RuleLanguage { get; } = RuleLanguages.CSharp;

        public string RuleSearchExpression { get; } = "**\\*.scs";

        public IEnumerable<ShapeFormat> InputFormats { get; } = new[]
        {
            ShapeFormat.Json,
            ShapeFormat.Clr,
            ShapeFormat.Xml,
            ShapeFormat.Yaml
        };

        public IEnumerable<ShapeFormat> OutputFormats { get; } = new[]
        {
            ShapeFormat.Json,
            ShapeFormat.Clr,
            ShapeFormat.Xml,
            ShapeFormat.Yaml
        };

        public async Task<Shape> Transform(Shape inputShape, ProjectionRule projectionRule,
            IDictionary<string, string> parameters)
        {
            var globals = new RoslynShapeFlowGlobals(inputShape);

            var scriptOptions = ScriptOptions.Default;
            scriptOptions = scriptOptions
                .WithImports("System", "System.Collections", "System.Collections.Generic", "ShapeFlow", "ShapeFlow.Shapes")
                .WithReferences(typeof(Shape).Assembly);

            var script = CSharpScript.Create<Shape>(
                projectionRule.Text, 
                globalsType: typeof(RoslynShapeFlowGlobals),
                options:scriptOptions);
            var runner = script.CreateDelegate();
            var result = await runner(globals);
            return result;
        }
    }
}
