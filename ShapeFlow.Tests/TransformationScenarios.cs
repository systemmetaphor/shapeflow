using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShapeFlow.Infrastructure;
using ShapeFlow;
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
        [DeploymentItem("Models\\Order.json", "Models")]
        [DeploymentItem("Templates\\DomainObjects.liquid", "Templates")]
        [DeploymentItem("Projects\\DDD.config.json")]
        public void SimpleModel2Code()
        {
            // MISSING
            // - process the output decl of the pipeline

            using (var container = ApplicationContainerFactory.Create(Application.RegisterComponents))
            {
                var parameters = new Dictionary<string, string>
                {
                    { "project-root", Environment.CurrentDirectory }
                };

                var solution = Solution.ParseFile("Projects\\DDD.config.json");
                solution.AddParameters(parameters);

                var engine = container.Activate<PipelineEngine>();
                engine.Process(new SolutionEventContext(solution));

                Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "Generated\\DomainObjects.cs")));
            }
        }

        [TestMethod]
        
        [DeploymentItem("Templates\\TablesToRecords.liquid", "Templates")]
        [DeploymentItem("Projects\\TablesToRecords.config.json")]
        public void Db2Code()
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

                var engine = container.Activate<PipelineEngine>();

                engine.Process(new SolutionEventContext(solution));

                Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "Generated\\Records.cs")));
            }
        }
    }
}
