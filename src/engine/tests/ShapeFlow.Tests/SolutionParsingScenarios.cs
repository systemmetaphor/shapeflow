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
        public void CanParseValidFile()
        {
            var solutionObject = JObject.Parse(File.ReadAllText("Projects\\valid.config.json"));
            var solution = SolutionDeclaration.Parse(solutionObject);

            Assert.IsNotNull(solution.Name);
                        
            Assert.IsTrue(solution.Projections.Any());
            Assert.IsTrue(solution.Shapes.Any());
            Assert.IsTrue(solution.Pipelines.Any());            
        }

        [TestMethod]
        [ExpectedException(typeof(SolutionParsingException))]
        public void EmptyObjectLiteralThrows()
        {
            var jsonText = "{}";
            SolutionDeclaration.Parse(jsonText);
        }
    }
}
