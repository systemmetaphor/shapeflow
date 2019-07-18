using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mono.TextTemplating;
using Newtonsoft.Json.Linq;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Projections;
using ShapeFlow.Shapes;

namespace ShapeFlow.TemplateEngines.T4
{
    public class T4ProjectionRuleEngine : IProjectionRuleEngine
    {
        private readonly TextTemplateProvider _fileProvider;
        private readonly IOutputLanguageInferenceService _inferenceService;

        public T4ProjectionRuleEngine(TextTemplateProvider fileProvider, IOutputLanguageInferenceService inferenceService)
        {
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            _inferenceService = inferenceService ?? throw new ArgumentNullException(nameof(inferenceService));

            RuleLanguage = TextTemplateLanguages.T4;
            RuleSearchExpression = ".\\**\\*.tt";
        }

        public string RuleLanguage { get; }

        public string RuleSearchExpression { get; }

        public ProjectionContext Transform(ProjectionContext projectionContext, ProjectionRuleDeclaration projectionRule)
        {
            if (projectionContext.Output.Model.Format != ShapeFormat.FileSet)
            {
                throw new InvalidOperationException($"The language {RuleLanguage} can only output shapes of {nameof(ShapeFormat.FileSet)} format.");
            }

            var outputSet = projectionContext.Output.Model.GetInstance() as FileSet;
            if (outputSet == null)
            {
                throw new InvalidOperationException($"You must set a non null {nameof(FileSet)} shape on the projection output shape.");
            }

            var templateFile = _fileProvider.GetFile(projectionContext, projectionRule);
            var templateFileText = templateFile.Text;

            string outputPath = null;
            var outputText = TransformCore(projectionContext, projectionRule.FileName, templateFileText, ref outputPath);

            if (string.IsNullOrWhiteSpace(outputPath) && !string.IsNullOrWhiteSpace(outputText))
            {
                var templateFileName = projectionRule.FileName;
                var languageExtension = _inferenceService.InferFileExtension(outputText);
                outputPath = Path.ChangeExtension(templateFileName, ".generated.txt");
                outputPath = Path.ChangeExtension(outputPath, languageExtension);
            }

            var f = new FileSetFile(outputText, outputPath);
            outputSet.AddFile(f);

            return projectionContext;
        }

        public string TransformString(ProjectionContext projectionContext, string inputText)
        {
            string outputPath = null;
            return TransformCore(projectionContext, string.Empty, inputText, ref outputPath);
        }

        private string TransformCore(ProjectionContext context, string templateName, string templateFileText, ref string outputPath)
        {
            var generator = new TemplateGenerator();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

                if (generator.Refs.Contains(assembly.Location))
                {
                    continue;
                }

                generator.Refs.Add(assembly.Location);
            }


            generator.AddDirectiveProcessor("PropertyProcessor", typeof(TemplateArgumentDirectiveProcessor).FullName,
                this.GetType().Assembly.FullName);

            PrepareInput(context);
            generator.ProcessTemplate(templateName, templateFileText, ref outputPath, out var outputText);

            if (generator.Errors.HasErrors)
            {
                var builder = new StringBuilder();

                foreach (CompilerError generatorError in generator.Errors)
                {
                    builder.AppendLine(generatorError.ErrorText);
                }

                throw new InvalidOperationException(builder.ToString());
            }

            return outputText;
        }

        private static void PrepareInput(ProjectionContext context)
        {
            var modelContainer = context.Input.Model;
            var inputs = new Dictionary<string, object>();
            var model = modelContainer.GetInstance();

            if (model is JObject modelJObject)
            {
                model = modelJObject.ToDictionary();
            }

            if (model is IDictionary<string, object> modelDictionary)
            {
                foreach (var kvp in modelDictionary)
                {
                    inputs.Add(kvp.Key, kvp.Value);
                }
            }

            foreach (var p in context.Solution.Parameters)
            {
                inputs.Add(p.Key, p.Value);
            }

            foreach (var i in inputs)
            {
                CallContext.SetData(i.Key, i.Value);
            }
        }
    }
}
