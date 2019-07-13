using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ShapeFlow.Declaration;

namespace ShapeFlow.Tests
{
    [TestClass]
    public class SolutionParsingScenarios
    {
        [TestMethod]
        [DeploymentItem("Projects\\valid.config.json")]
        [DeploymentItem("Models\\customers.template.xlsx", "importexport")]
        [DeploymentItem("Models\\partners.template.xlsx", "importexport")]
        [DeploymentItem("Models\\employees.template.xlsx", "importexport")]

        public void CanParseValidFile()
        {
            var solutionObject = JObject.Parse(File.ReadAllText("Projects\\valid.config.json"));
            var solution = Solution.Parse(solutionObject);

            Assert.IsNotNull(solution.Name);
                        
            Assert.IsTrue(solution.Projections.Count() > 0);
            Assert.IsTrue(solution.Models.Count() > 0);
            Assert.IsTrue(solution.Pipelines.Count() > 0);            
        }
    }
}
