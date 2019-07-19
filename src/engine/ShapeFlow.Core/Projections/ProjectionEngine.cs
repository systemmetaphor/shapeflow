using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            RuleLanguageProvider ruleLanguageProvider,
            IOutputLanguageInferenceService inferenceService,
            ProjectionRuleProvider ruleProvider)
        {            
            InputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            FileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            RuleLanguageProvider = ruleLanguageProvider ?? throw new ArgumentNullException(nameof(ruleLanguageProvider));
            InferenceService = inferenceService ?? throw new ArgumentNullException(nameof(inferenceService));
            RuleProvider = ruleProvider ?? throw new ArgumentNullException(nameof(ruleProvider));
        }
        
        protected ShapeManager InputManager { get; }

        protected IFileService FileService { get; }

        protected ProjectionRuleProvider RuleProvider { get; }

        protected RuleLanguageProvider RuleLanguageProvider { get; }

        protected  IOutputLanguageInferenceService InferenceService { get; }

        public async Task<ProjectionContext> Transform(ProjectionContext projectionContext)
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
                    var fileSetOutput = new FileSetShape(projectionContext.PipelineDeclaration.Name);
                    var outputShapeDecl = new ShapeDeclaration(
                        projectionContext.PipelineDeclaration.Name,
                        typeof(ProjectionEngine).FullName,
                        Enumerable.Empty<string>(),
                        new Dictionary<string, string>());

                    projectionContext.Output = new ShapeContext(outputShapeDecl, fileSetOutput);

                    break;
                    
                default:
                    break;
            }

            var inputShape = projectionContext.Input.Shape;
            
            var projectionRules = projectionDecl.Rules;
            foreach (var projectionRuleDecl in projectionRules)
            {
                var projectionRule = RuleProvider.GetFile(projectionContext, projectionRuleDecl);
                var templateEngine = RuleLanguageProvider.GetEngine(projectionRuleDecl.Language);

                var outputShape = await templateEngine.Transform(inputShape, projectionRule, projectionContext.Solution.Parameters);

                if (format == ShapeFormat.FileSet)
                {
                    if (!(projectionContext.Output.Shape.GetInstance() is FileSet outputSet))
                    {
                        throw new InvalidOperationException($"You must set a non null {nameof(FileSet)} shape on the projection output shape.");
                    }

                    var stringOutput = (StringShape) outputShape;
                    var output = (string)stringOutput.GetInstance();
                    var templateFileName = projectionRuleDecl.FileName;
                    var languageExtension = InferenceService.InferFileExtension(output);
                    var outputPath = Path.ChangeExtension(templateFileName, ".generated.txt");
                    outputPath = Path.ChangeExtension(outputPath, languageExtension);

                    var result = new FileSetFile(output, outputPath);
                    outputSet.AddFile(result);
                }
            }

            // end gen impl
           
            return projectionContext;
        }
    }
}
