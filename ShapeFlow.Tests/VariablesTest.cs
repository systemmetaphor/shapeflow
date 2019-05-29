using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ShapeFlow.ModelDriven;

namespace ShapeFlow.Tests
{
    [TestClass]
    public class VariablesTest
    {
        [TestMethod]
        public void CanResolveVariable()
        {
            string variablesText = @"{
    ""project-path"" : ""c:\\something""
}";

            string toResolve = "$(project-path)\\domain\\generated.cs";

            var variables = new Variables(JObject.Parse(variablesText));

            var result = variables.Resolve(toResolve);

            Assert.AreEqual("c:\\something\\domain\\generated.cs", result);
        }
    }
}
