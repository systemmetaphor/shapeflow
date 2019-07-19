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
        private readonly TextTemplateProvider _fileProvider;

        public RoslynRuleEngine(TextTemplateProvider fileProvider)
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

        public async Task<ProjectionContext> Transform(ProjectionContext projectionContext, ProjectionRuleDeclaration projectionRule)
        {
            var globals = new RoslynShapeFlowGlobals(projectionContext);
            var templateFile = _fileProvider.GetFile(projectionContext, projectionRule);
            var templateFileText = templateFile.Text;

            var script = CSharpScript.Create<ProjectionContext>(
                templateFileText, 
                globalsType: typeof(RoslynShapeFlowGlobals),
                options:ScriptOptions.Default.WithImports("System", "System.Collections", "System.Collections.Generic"));
            var runner = script.CreateDelegate();
            var result = await runner(globals);
            return result;
        }

        public Task<string> TransformString(ProjectionContext projectionContext, string inputText)
        {
            throw new NotSupportedException();
        }
    }

    public class RoslynShapeFlowGlobals
    {
        public RoslynShapeFlowGlobals(ProjectionContext ctx)
        {
            Context = ctx;
        }

        public  ProjectionContext Context { get; }

        public InputDeclaration InputDeclaration => Context.PipelineDeclaration.Input;

        public OutputDeclaration OutputDeclaration => Context.PipelineDeclaration.Output;

        public ShapeContext InputContext => Context.Input;

        public ShapeContext OuputContext => Context.Output;

        public object Input => InputContext.Shape.GetInstance();
    }
}
