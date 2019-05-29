using System;
using System.IO;
using ShapeFlow.Infrastructure;
using ShapeFlow.ModelDriven.Models;
using Newtonsoft.Json;

namespace ShapeFlow.ModelDriven
{
    public class ModelToTextProjectionEngine
    {
        public ModelToTextProjectionEngine(            
            ILoggingService loggingService,
            IModelManager inputManager,
            IFileService fileService,
            ITemplateEngineProvider templateEngineProvider,
            TextGeneratorRegistry generatorRegistry)
        {
            LoggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            InputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            FileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            TemplateEngineProvider = templateEngineProvider ?? throw new ArgumentNullException(nameof(templateEngineProvider));
            GeneratorRegistry = generatorRegistry;
        }

        protected ILoggingService LoggingService { get; }

        protected IModelManager InputManager { get; }

        protected IFileService FileService { get; }

        protected ITemplateEngineProvider TemplateEngineProvider { get; }

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

                LoggingService.Info("Projection completed.");
            }

            return context;
        }
    }
}
