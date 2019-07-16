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
        [DeploymentItem("ShapeDeclarations\\Order.json", "ShapeDeclarations")]
        [DeploymentItem("Templates\\DomainObjects.liquid", "Templates")]
        [DeploymentItem("Projects\\DDD.config.json")]
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

            Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "Generated\\DomainObjects.cs")));
        }

        [TestMethod]
        [TestCategory("ExcludeFromBuildServer")]
        [DeploymentItem("Templates\\TablesToRecords.liquid", "Templates")]
        [DeploymentItem("Projects\\TablesToRecords.config.json")]
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

                Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "Generated\\Records.cs")));
            }
        }
    }
}
