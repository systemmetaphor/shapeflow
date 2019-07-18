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
            RuleLanguageProvider ruleLanguageProvider)
        {            
            InputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            FileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            RuleLanguageProvider = ruleLanguageProvider ?? throw new ArgumentNullException(nameof(ruleLanguageProvider));
        }
        
        protected ShapeManager InputManager { get; }

        protected IFileService FileService { get; }

        protected RuleLanguageProvider RuleLanguageProvider { get; }

        public ProjectionContext Transform(ProjectionContext projectionContext)
        {
            // this the generator impl

            var projectionRules = projectionContext.PipelineDeclaration.Projection.Rules;

            // based on the projection expression determine the output shape

            var transformationOutput = new FileSetShape(projectionContext.PipelineDeclaration.Name);
            var outputShapeDecl = new ShapeDeclaration(
                projectionContext.PipelineDeclaration.Name,
                typeof(ProjectionEngine).FullName,
                Enumerable.Empty<string>(),
                new Dictionary<string, string>());
            
            projectionContext.Output = new ShapeContext(outputShapeDecl, transformationOutput);
            
            foreach (var projectionRule in projectionRules)
            {
                var templateEngine = RuleLanguageProvider.GetEngine(projectionRule.Language);
                projectionContext = templateEngine.Transform(projectionContext, projectionRule);
            }

            // end gen impl
           
            return projectionContext;
        }
    }
}
