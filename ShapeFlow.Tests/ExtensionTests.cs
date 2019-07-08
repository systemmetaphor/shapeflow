using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShapeFlow.Declaration;

namespace ShapeFlow.Tests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void ExtensionLoadingHappyPath()
        {
            // the solution declares the assembly via package / version

            var solution = Solution.ParseFile("Projects\\extension.config.json");
            var projection = solution.Projections.FirstOrDefault();
            Assert.IsNotNull(projection);
            Assert.IsFalse(string.IsNullOrWhiteSpace(projection.PackageName));
            Assert.IsFalse(string.IsNullOrWhiteSpace(projection.PackageVersion));

            // find the extension class and activate it
                        
            var parameters = new Dictionary<string, string>
            {
                { "project-root", Environment.CurrentDirectory },
                { "project", "Projects\\extension.config.json" }
            };

            solution.AddParameters(parameters);

            using(var container = ApplicationContainerFactory.Create(Application.Register))
            {
                var engine = container.Resolve<ShapeFlowEngine>();
                engine.Run(new SolutionEventContext(solution));
            }

            // get the metadata, parse it and register the parts comming from the extension



            // assemble and run

            // verify
        }
    }
}
