using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.PackageManagement;

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

            //var basePath = Path.Combine(testBedHelper.ShapeflowFolder, "ShapeFlow.Projections.Persistence.1.0.0");
            //if (!Directory.Exists(basePath))
            //{
            //    Directory.CreateDirectory(basePath);
            //}

            //File.Copy("Projections\\ShapeFlow.Projections.Persistence.1.0.0.nupkg", Path.Combine(basePath, "ShapeFlow.Projections.Persistence.1.0.0.nupkg"), true);

            //using (var stream = File.OpenRead(a1Package))
            //{
            //    var downloadResult = new DownloadResourceResult(stream, packagesSourceDirectory);
            //    await msBuildProject.InstallPackageAsync(
            //        a1,
            //        downloadResult,
            //        testNuGetProjectContext,
            //        CancellationToken.None);
            //}

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
                var managerFactory = container.Resolve<PackageManagerFactory>();
                

                // assemble the engine an run it
                var engine = container.Resolve<ShapeFlowEngine>();
                var solution = SolutionDeclaration.ParseFile(parameters);

                var packageManager = managerFactory.Create(solution);

                await packageManager.TryInstallPackage("Projections\\ShapeFlow.Projections.Persistence.1.0.0.nupkg");

                await engine.Run(solution);
            }

            Assert.IsTrue(testBedHelper.FileExists("Records\\records.generated.cs"));
        }
    }
}
