using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShapeFlow.Infrastructure;
using ShapeFlow;
using ShapeFlow.Declaration;
using ShapeFlow.Pipelines;

namespace ShapeFlow.Tests
{
    [TestClass]
    public class TransformationScenarios
    {
        public TestContext TestContext
        {
            get;
            set;
        }

        [TestMethod]
        public async Task SimpleModel2Code()
        {
            // MISSING
            // - process the output decl of the pipeline

            var parameters = new Dictionary<string, string>
                {
                    { "project-root", Environment.CurrentDirectory },
                    { "project", "Projects\\DDD.config.json" }
                };

            using (var container = ApplicationContainerFactory.Create(Application.RegisterComponents))
            {
                var engine = container.Activate<ShapeFlowEngine>();
                await engine.Run(parameters);
            }

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "DomainObjects\\Aggregates.generated.cs")));
        }

        [TestMethod]
        [TestCategory("ExcludeFromBuildServer")]
        public async Task Db2Code()
        {
            // MISSING
            // - process the output decl of the pipeline

            using (var container = ApplicationContainerFactory.Create(Application.RegisterComponents))
            {
                var parameters = new Dictionary<string, string>
                {
                    { "project-root", Environment.CurrentDirectory }
                };

                var solution = Solution.ParseFile("Projects\\TablesToRecords.config.json");
                solution.AddParameters(parameters);

                var engine = container.Activate<ShapeFlowEngine>();

                await engine.Run(solution);

                Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "Data\\Records.generated.cs")));
            }
        }
    }
}
