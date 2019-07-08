using System;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Shapes;

namespace ShapeFlow.Projections
{
    public class ModelToTextProjectionEngine
    {
        public ModelToTextProjectionEngine(            
            ShapeManager inputManager,
            IFileService fileService,
            TemplateEngineProvider templateEngineProvider,
            TextGeneratorRegistry generatorRegistry)
        {            
            InputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            FileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            TemplateEngineProvider = templateEngineProvider ?? throw new ArgumentNullException(nameof(templateEngineProvider));
            GeneratorRegistry = generatorRegistry;
        }
        
        protected ShapeManager InputManager { get; }

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
                               
                context.Output = Transform(context, input);

                AppTrace.Information("Projection completed.");
            }

            return context;
        }

        public ModelToTextOutput Transform(ProjectionContext context, ProjectionInput input)
        {
            // this the generator impl

            GeneratorRegistry.TryGet(context.GeneratorName, out ProjectionDeclaration generatorDecl);

            var transformationRules = generatorDecl.Rules;

            var transformationOutput = new ModelToTextOutput();

            foreach (var tranformationRule in transformationRules)
            {
                var templateEngine = TemplateEngineProvider.GetEngine(tranformationRule.TemplateLanguage);
                var transformationOutputFile = templateEngine.Transform(context, input, tranformationRule);
                transformationOutput.AddOutputFile(transformationOutputFile);
            }

            // end gen impl

            return transformationOutput;
        }
    }
}
