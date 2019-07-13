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
            ProjectionRegistry generatorRegistry)
        {            
            InputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            FileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            TemplateEngineProvider = templateEngineProvider ?? throw new ArgumentNullException(nameof(templateEngineProvider));
            GeneratorRegistry = generatorRegistry;
        }
        
        protected ShapeManager InputManager { get; }

        protected IFileService FileService { get; }

        protected TemplateEngineProvider TemplateEngineProvider { get; }

        protected ProjectionRegistry GeneratorRegistry { get; }

        public PipelineContext Transform(PipelineContext pipelineContext)
        {
            var modelManager = InputManager;

            var model = modelManager.Get(pipelineContext.Input.Selector);
            if (model != null)
            {
                var input = new ProjectionInput(model);
                               
                pipelineContext.Output = Transform(pipelineContext, input);

                AppTrace.Information("Projection completed.");
            }

            return pipelineContext;
        }

        public ModelToTextOutput Transform(PipelineContext pipelineContext, ProjectionInput input)
        {
            // this the generator impl

            GeneratorRegistry.TryGet(pipelineContext.GeneratorName, out ProjectionDeclaration projection);

            var transformationRules = projection.Rules;

            var transformationOutput = new ModelToTextOutput();

            foreach (var tranformationRule in transformationRules)
            {
                var templateEngine = TemplateEngineProvider.GetEngine(tranformationRule.TemplateLanguage);
                var transformationOutputFile = templateEngine.Transform(pipelineContext, input, projection, tranformationRule);
                transformationOutput.AddOutputFile(transformationOutputFile);
            }

            // end gen impl

            return transformationOutput;
        }
    }
}
