using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Loaders;
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
            ProjectionRuleProvider ruleProvider, 
            LoaderRegistry loaders)
        {            
            InputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            FileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            RuleLanguageProvider = ruleLanguageProvider ?? throw new ArgumentNullException(nameof(ruleLanguageProvider));
            InferenceService = inferenceService ?? throw new ArgumentNullException(nameof(inferenceService));
            RuleProvider = ruleProvider ?? throw new ArgumentNullException(nameof(ruleProvider));
            Loaders = loaders ?? throw new ArgumentNullException(nameof(loaders));
        }
        
        protected ShapeManager InputManager { get; }

        protected IFileService FileService { get; }

        protected ProjectionRuleProvider RuleProvider { get; }

        protected RuleLanguageProvider RuleLanguageProvider { get; }

        protected  IOutputLanguageInferenceService InferenceService { get; }

        protected LoaderRegistry Loaders { get; }

        public async Task<ProjectionContext> Transform(ProjectionContext projectionContext)
        {
            var pipelineDecl = projectionContext.PipelineDeclaration;

            // the projection indicates the input and output formats
            var projectionDecl = pipelineDecl.Projection;

            // input validation, do some simple sanity checks

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

            if (!Enum.TryParse(pipelineDecl.Output.OutputType, true, out ShapeFormat outputFormat))
            {
                throw new InvalidOperationException("Invalid output format");
            }

            // now that we have the format we must create the resulting shape instance

            if (projectionContext.Output == null)
            {
                var outputShapeDecl = new ShapeDeclaration(
                    projectionContext.PipelineDeclaration.Name,
                    pipelineDecl.Output.OutputType,
                    Enumerable.Empty<string>(),
                    new Dictionary<string, string>());

                Loaders.TryGet(pipelineDecl.Output.OutputType, out ILoader loader);

                projectionContext.Output = loader.Create(outputShapeDecl);
            }

            // run the projections rules
            var projectionRules = projectionDecl.Rules;
            foreach (var projectionRuleDecl in projectionRules)
            {
                var projectionRule = RuleProvider.GetFile(projectionContext, projectionRuleDecl);
                var templateEngine = RuleLanguageProvider.GetEngine(projectionRuleDecl.Language);

                // if the projection expression is not set at the rule use the general one defined at the projection
                var projectionExpressionText = projectionRuleDecl.ProjectionExpression;
                if (string.IsNullOrWhiteSpace(projectionExpressionText))
                {
                    projectionExpressionText = projectionDecl.ProjectionExpression;
                }

                var projectionExpression = ProjectionExpressionParser.Parse(projectionExpressionText);

                Shape inputShape;

                // the empty selector means passing in the input set (superset)
                if (projectionExpression.Source.IsSuperSetSelector)
                {
                    inputShape = projectionContext.Input.Shape;
                }
                else
                {
                    // otherwise we must generate a subset from the input set
                    throw new NotSupportedException();
                }
                
                var outputShape = await templateEngine.Transform(inputShape, projectionRule, projectionContext.Solution.Parameters);

                // we now have an output shape telling us its format and optionally name and tags
                
                // by convention if the output of the projection is a fileset and the rule is giving a string then 
                // the string will become a file and the file name will be derived from the rule name
                if (projectionContext.Output.Shape.GetInstance() is FileSet outputSet && outputShape is StringShape stringOutput)
                {
                        var output = (string) stringOutput.GetInstance();
                        var templateFileName = projectionRuleDecl.FileName;
                        var languageExtension = InferenceService.InferFileExtension(output);
                        var outputPath = Path.ChangeExtension(templateFileName, ".generated.txt");
                        outputPath = Path.ChangeExtension(outputPath, languageExtension);
                        var fileShape = new FileShape(output, outputPath);
                        outputShape = fileShape;
                        outputSet.Add(fileShape);
                }
                else
                {
                    var add = projectionContext.Output.Shape.GetType().GetMethod("Add");
                    if (add == null)
                    {
                        throw new InvalidOperationException();
                    }

                    add.Invoke(projectionContext.Output.Shape, new object[] { outputShape });
                }
            }

            // end gen impl
           
            return projectionContext;
        }
    }
}
