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
using ShapeFlow.RuleEngines.T4;
using ShapeFlow.Shapes;

namespace ShapeFlow.Tests
{
    [TestClass]
    public class T4TemplateEngineTests
    {
        [TestMethod]
        public async Task CanPerformSimpleTransformation()
        {
            using (var currentContainer = ApplicationContainerFactory.Create(Application.Register))
            {
                var loader = currentContainer.Resolve<ShapeManager>();

                var engine = new T4ProjectionRuleEngine();

                var templateText = File.ReadAllText("Rules\\DomainObjects\\Aggregates.tt");
                var solution = SolutionDeclaration.ParseFile("Projects\\DDD.config.json");
                var shapeContext = await loader.GetOrLoad(solution.Shapes.First());

                var ctx = new ProjectionContext(solution.Pipelines.First(), solution.Pipelines.First().Stages.First(), shapeContext);
                var result = await engine.Transform(shapeContext.Shape, ProjectionRule.Create(templateText, RuleLanguages.T4), new Dictionary<string, string>());

                Assert.IsNotNull(result);

                var resultText = (string)result.GetInstance();

                Assert.IsTrue(resultText.Contains("class Order"));
            }
        }
    }
}
