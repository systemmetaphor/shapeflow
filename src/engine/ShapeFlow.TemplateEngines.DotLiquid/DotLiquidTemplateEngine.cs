using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DotLiquid;
using DotLiquid.NamingConventions;
using Newtonsoft.Json.Linq;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Projections;

namespace ShapeFlow.TemplateEngines.DotLiquid
{
    public class DotLiquidTemplateEngine : ITextTemplateEngine
    {
        private static BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        private const string HashStateKey = "DOTLIQUID_HASH";

        private readonly TextTemplateProvider _fileProvider;
        private readonly IOutputLanguageInferenceService _inferenceService;


        public DotLiquidTemplateEngine(TextTemplateProvider fileProvider, IOutputLanguageInferenceService inferenceService)
        {
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            _inferenceService = inferenceService ?? throw new ArgumentNullException(nameof(inferenceService));
            TemplateSearchExpression = ".\\**\\*.liquid";
        }

        public string TemplateLanguage => TextTemplateLanguages.DotLiquid;

        public string TemplateSearchExpression { get; }

        public ModelToTextOutputFile Transform(ProjectionContext projectionContext, ProjectionRuleDeclaration projectionRule)
        {
            var hash = projectionContext.GetStateEntry<Hash>(HashStateKey);
            if (hash == null)
            {
                hash = PrepareHash(projectionContext.Input);
                projectionContext.AddStateEntry(HashStateKey, hash);
            }

            var templateFile = _fileProvider.GetFile(projectionContext, projectionRule);
            ModelToTextOutputFile result;
            try
            {
                var template = Template.Parse(templateFile.Text);
                var output = template.Render(hash);

                string outputPath;

                if (string.IsNullOrWhiteSpace(projectionRule.OutputPathTemplate))
                {
                    var templateFileName = projectionRule.TemplateName;
                    var languageExtension = _inferenceService.InferFileExtension(output);
                    outputPath = Path.ChangeExtension(templateFileName, ".generated.txt");
                    outputPath = Path.ChangeExtension(outputPath, languageExtension);
                }
                else
                {
                    var nameTemplate = Template.Parse(projectionRule.OutputPathTemplate);
                    outputPath = nameTemplate.Render(hash);
                }

                result = new ModelToTextOutputFile(output, outputPath);
            }
            catch (Exception e)
            {
                AppTrace.Error("Template processing failed:", e);
                throw;
            }

            return result;
        }

        public string TransformString(ProjectionContext projectionContext,  string outputPathRule)
        {
            var hash = PrepareHash(projectionContext.Input);
            var template = Template.Parse(outputPathRule);
            var output = template.Render(hash);
            return output;
        }

        private static Hash PrepareHash(ShapeContext projectionInput)
        {
            Hash hash = null;
            if (projectionInput == null)
            {
                hash = new Hash();
                return hash;
            }

            var modelContainer = projectionInput.Model;

            if (modelContainer.Format == ShapeFormat.Clr)
            {
                // on CLR models we need to configure the engine to allow the public properties
                // of the model objects
                PrepareDotLiquidEngine(modelContainer.GetType());
            }

            Template.NamingConvention = new CSharpNamingConvention();

            var model = modelContainer.GetInstance();

            if (model is JObject modelJObject)
            {
                model = modelJObject.ToDictionary();
            }

            if (model is IDictionary<string, object> modelDictionary)
            {
                hash = Hash.FromDictionary(modelDictionary);
            }

            if (hash == null)
            {
                hash = Hash.FromAnonymousObject(modelContainer);
            }

            foreach (var p in projectionInput.Parameters)
            {
                if (!hash.ContainsKey(p.Key))
                {
                    hash.Add(p.Key, p.Value);
                }
            }

            return hash;
        }

        private static void PrepareDotLiquidEngine(Type rootType)
        {
            var propertiesToConsider = rootType.GetProperties(_bindingFlags);

            var simpleProperties = new List<string>();

            foreach (var property in propertiesToConsider)
            {
                simpleProperties.Add(property.Name);

                if (!property.PropertyType.IsSimpleType())
                {
                    if (!property.PropertyType.TryGetCollectionElementType(out Type elementType))
                    {
                        elementType = property.PropertyType;
                    }

                    PrepareDotLiquidEngine(elementType);
                }
            }

            Template.RegisterSafeType(rootType, simpleProperties.ToArray());
        }
    }
}
