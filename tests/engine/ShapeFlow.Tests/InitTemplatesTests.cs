using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShapeFlow.Declaration;

namespace ShapeFlow.Tests
{
    [TestClass]
    public class InitTemplatesTests
    {
        [TestMethod]
        public void CanGenerateTemplate()
        {
            var text = InitTemplate.Generate(new InitTemplateOptions { ProjectName =  "test", RootDirectory = Environment.CurrentDirectory });

            Assert.IsNotNull(text);
        }
    }
}
