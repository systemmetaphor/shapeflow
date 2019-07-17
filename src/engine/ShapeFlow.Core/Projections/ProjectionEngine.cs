using System;
using System.Collections.Generic;
using System.Linq;
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

        public ProjectionContext Transform(ProjectionContext projectionContext)
        {
            // this the generator impl

            var projectionRules = projectionContext.PipelineDeclaration.Projection.Rules;

            var transformationOutput = new FileSetShape(projectionContext.PipelineDeclaration.Name);

            foreach (var projectionRule in projectionRules)
            {
                var templateEngine = TemplateEngineProvider.GetEngine(projectionRule.TemplateLanguage);
                var transformationOutputFile = templateEngine.Transform(projectionContext, projectionRule);
                transformationOutput.FileSet.AddFile(transformationOutputFile);
            }

            // end gen impl

            var outputShapeDecl = new ShapeDeclaration(
                projectionContext.PipelineDeclaration.Name, 
                typeof(ProjectionEngine).FullName, 
                Enumerable.Empty<string>(), 
                new Dictionary<string, string>());

            // problem with the Shape Declaration
            projectionContext.Output = new ShapeContext(outputShapeDecl, transformationOutput);

            return projectionContext;
        }
    }
}
