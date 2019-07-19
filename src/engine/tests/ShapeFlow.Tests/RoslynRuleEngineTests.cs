using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Output;
using ShapeFlow.Projections;
using ShapeFlow.RuleEngines.Roslyn;
using ShapeFlow.RuleEngines.T4;
using ShapeFlow.Shapes;

namespace ShapeFlow.Tests
{
    [TestClass]
    public class RoslynRuleEngineTests
    {
        [TestMethod]
        public async Task CanPerformSimpleTransformation()
        {
            using (var currentContainer = ApplicationContainerFactory.Create(Application.Register))
            {
                var templateProvider = currentContainer.Resolve<TextTemplateProvider>();
                var loader = currentContainer.Resolve<ShapeManager>();
                var projectionRegistry = currentContainer.Resolve<ProjectionRegistry>();
                var shapeflow = currentContainer.Resolve<ShapeFlowEngine>();

                var engine = new RoslynRuleEngine(templateProvider);
                
                var solution = Solution.ParseFile("Projects\\M2M.config.json");
                var shapeContext = loader.GetOrLoad(solution.ShapeDeclarations.First());

                solution = await projectionRegistry.Process(solution);
                var pipeline = shapeflow.GetOrAssemblePipeline(solution);

                var ctx = new ProjectionContext(solution, solution.Pipelines.First(), shapeContext);
                var result = await engine.Transform(ctx, new ProjectionRuleDeclaration("M2M\\Simple.scs"));

                Assert.IsNotNull(result);
            }
        }
    }
}
