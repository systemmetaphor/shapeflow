using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ShapeFlow.Tests
{
    [TestClass]
    public class ExtensionTests
    {
        private const string ExtensionsProjectFilePath = "Projects\\extension.config.json";

        [TestMethod]
        [TestCategory("ExcludeFromBuildServer")]
        public async Task ExtensionLoadingHappyPath()
        {
            // create the test bed project

            var testBedHelper = new SolutionTestBedHelper();
            testBedHelper.Create();

            testBedHelper.AddFile(ExtensionsProjectFilePath);
            var projectFileName = Path.GetFileName(ExtensionsProjectFilePath);

            // generate the parameters object
                        
            var parameters = new Dictionary<string, string>
            {
                { "project-root", testBedHelper.SolutionDir },
                { "project", projectFileName }
            };

            using(var container = ApplicationContainerFactory.Create(Application.Register))
            {
                // assemble the engine an run it
                var engine = container.Resolve<ShapeFlowEngine>();
                await engine.Run(parameters);
            }

            Assert.IsTrue(testBedHelper.FileExists("Records\\records.generated.cs"));
        }
    }
}
