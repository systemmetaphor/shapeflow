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
    public class T4TemplateEngine : ITextTemplateEngine
    {
        private readonly TextTemplateProvider _fileProvider;
        private readonly IOutputLanguageInferenceService _inferenceService;

        public T4TemplateEngine(TextTemplateProvider fileProvider, IOutputLanguageInferenceService inferenceService)
        {
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            _inferenceService = inferenceService ?? throw new ArgumentNullException(nameof(inferenceService));

            TemplateLanguage = TextTemplateLanguages.T4;
            TemplateSearchExpression = ".\\**\\*.tt";
        }

        public string TemplateLanguage { get; }

        public string TemplateSearchExpression { get; }

        public FileSetFile Transform(ProjectionContext projectionContext, ProjectionRuleDeclaration projectionRule)
        {
            var templateFile = _fileProvider.GetFile(projectionContext, projectionRule);
            var templateFileText = templateFile.Text;

            string outputPath = null;
            var outputText = TransformCore(projectionContext, projectionRule.TemplateName, templateFileText, ref outputPath);

            if (string.IsNullOrWhiteSpace(outputPath) && !string.IsNullOrWhiteSpace(outputText))
            {
                var templateFileName = projectionRule.TemplateName;
                var languageExtension = _inferenceService.InferFileExtension(outputText);
                outputPath = Path.ChangeExtension(templateFileName, ".generated.txt");
                outputPath = Path.ChangeExtension(outputPath, languageExtension);
            }

            return new FileSetFile(outputText, outputPath);
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
    }
}
