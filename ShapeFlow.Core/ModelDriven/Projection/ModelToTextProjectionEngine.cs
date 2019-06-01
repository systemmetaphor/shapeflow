using System;
using System.IO;
using ShapeFlow.Infrastructure;
using ShapeFlow.Models;
using Newtonsoft.Json;

namespace ShapeFlow
{
    public class ModelToTextProjectionEngine
    {
        public ModelToTextProjectionEngine(            
            ModelManager inputManager,
            IFileService fileService,
            TemplateEngineProvider templateEngineProvider,
            TextGeneratorRegistry generatorRegistry)
        {            
            InputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            FileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            TemplateEngineProvider = templateEngineProvider ?? throw new ArgumentNullException(nameof(templateEngineProvider));
            GeneratorRegistry = generatorRegistry;
        }
        
        protected ModelManager InputManager { get; }

        protected IFileService FileService { get; }

        protected TemplateEngineProvider TemplateEngineProvider { get; }

        protected TextGeneratorRegistry GeneratorRegistry { get; }

        public ProjectionContext Transform(ProjectionContext context)
        {
            var modelManager = InputManager;

            var model = modelManager.Get(context.Input.Selector);
            if (model != null)
            {
                var input = new ProjectionInput(model);

                // this the generator impl

                GeneratorRegistry.TryGet(context.GeneratorName, out GeneratorDeclaration generatorDecl);

                var transformationRules = generatorDecl.Rules;

                var transformationOutput = new ModelToTextOutput();

                foreach (var tranformationRule in transformationRules)
                {
                    var templateEngine = TemplateEngineProvider.GetEngine(tranformationRule.TemplateLanguage);
                    var transformationOutputFile = templateEngine.Transform(context, input, tranformationRule);
                    transformationOutput.AddOutputFile(transformationOutputFile);
                }

                // end gen impl

                context.Output = transformationOutput;

                AppTrace.Information("Projection completed.");
            }

            return context;
        }
    }
}
