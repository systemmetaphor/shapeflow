using System.Collections.Generic;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShapeFlow.Loaders.KriativityReflectedModel;

namespace ShapeFlow.Tests
{
    [TestClass]
    public class ReflectionTests
    {
        [TestMethod]
        public void CanDetectStateClasses()
        {
            var def = ModuleDefMD.Load(this.GetType().Assembly.Location);
            Assert.IsNotNull(def);

            var loader = new KriativityReflectedModelLoader();
            var root = loader.ReflectModel(null, def.GetTypes().ToDictionary(t => t.FullName, t => t));

            Assert.IsNotNull(root);

            Assert.IsTrue(root.HasBusinessObject("Object1Model"));
        }
    }

    public class Object1State
    {
        public string Name { get; set; }
    }

    public class Object1Data
    {
        public  string Name { get; set; }
    }
}