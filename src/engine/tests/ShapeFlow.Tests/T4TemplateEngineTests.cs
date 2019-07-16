﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Projections;
using ShapeFlow.Shapes;
using ShapeFlow.TemplateEngines.T4;

namespace ShapeFlow.Tests
{
    [TestClass]
    public class T4TemplateEngineTests
    {
        [TestMethod]
        public void CanPerformSimpleTransformation()
        {
            using (var currentContainer = ApplicationContainerFactory.Create(Application.Register))
            {
                var inference = currentContainer.Resolve<IOutputLanguageInferenceService>();
                var templateProvider = currentContainer.Resolve<TextTemplateProvider>();
                var loader = currentContainer.Resolve<ShapeManager>();

                var engine = new T4TemplateEngine(templateProvider, inference);

                var templateText = File.ReadAllText("Templates\\SimpleT4.tt");
                var solution = Solution.ParseFile("Projects\\DDD.config.json");
                var shapeContext = loader.GetOrLoad(solution.ShapeDeclarations.First());

                var ctx = new ProjectionContext(solution, solution.Pipelines.First(), shapeContext);
                var result = engine.TransformString(ctx, templateText);

                Assert.IsNotNull(result);

                Assert.IsTrue(result.Contains("class Order"));
            }
        }
    }
}
