using System;
using System.Collections.Generic;
using System.Linq;
using ShapeFlow.Declaration;
using ShapeFlow.Output;
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
            var pipelineDecl = projectionContext.PipelineDeclaration;
            var projectionDecl = pipelineDecl.Projection;

            // input validation

            if (projectionContext.Input?.Shape == null)
            {
                throw new InvalidOperationException("A non-null shape is required for a transformation to happen.");
            }

            // verify if the shape matches the selector

            if (!projectionContext.PipelineDeclaration.Input.Selector.Equals(projectionContext.Input.Shape.Name))
            {
                throw new InvalidOperationException("An internal error occured, the received input violates the pipeline selector.");
            }

            // based on the projection declaration determine the output shape

            if (!Enum.TryParse(pipelineDecl.Output.OutputType, true, out ShapeFormat format))
            {
                throw new InvalidOperationException("Invalid output format");
            }

            switch (format)
            {
                case ShapeFormat.FileSet:
                    var transformationOutput = new FileSetShape(projectionContext.PipelineDeclaration.Name);
                    var outputShapeDecl = new ShapeDeclaration(
                        projectionContext.PipelineDeclaration.Name,
                        typeof(ProjectionEngine).FullName,
                        Enumerable.Empty<string>(),
                        new Dictionary<string, string>());

                    projectionContext.Output = new ShapeContext(outputShapeDecl, transformationOutput);

                    break;
                    
                default:
                    throw new InvalidOperationException("Unsupported format");
            }
            

            
            var projectionRules = projectionDecl.Rules;
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
