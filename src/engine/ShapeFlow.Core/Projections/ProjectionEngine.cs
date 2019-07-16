using System;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Shapes;

namespace ShapeFlow.Projections
{
    public class ProjectionEngine
    {
        public ProjectionEngine(            
            ShapeManager inputManager,
            IFileService fileService,
            TemplateEngineProvider templateEngineProvider)
        {            
            InputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            FileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            TemplateEngineProvider = templateEngineProvider ?? throw new ArgumentNullException(nameof(templateEngineProvider));
        }
        
        protected ShapeManager InputManager { get; }

        protected IFileService FileService { get; }

        protected TemplateEngineProvider TemplateEngineProvider { get; }

        public ModelToTextOutput Transform(ProjectionContext projectionContext)
        {
            // this the generator impl

            var projectionRules = projectionContext.PipelineDeclaration.Projection.Rules;

            var transformationOutput = new ModelToTextOutput();

            foreach (var projectionRule in projectionRules)
            {
                var templateEngine = TemplateEngineProvider.GetEngine(projectionRule.TemplateLanguage);
                var transformationOutputFile = templateEngine.Transform(projectionContext, projectionRule);
                transformationOutput.AddOutputFile(transformationOutputFile);
            }

            // end gen impl

            return transformationOutput;
        }
    }
}
