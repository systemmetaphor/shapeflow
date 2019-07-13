using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DotLiquid;
using DotLiquid.NamingConventions;
using Newtonsoft.Json.Linq;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Projections.DotLiquid
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
        }

        public string TemplateLanguage => TextTemplateLanguages.DotLiquid;

        public ModelToTextOutputFile Transform(PipelineContext pipelineContext, ProjectionInput input, ProjectionDeclaration projection, ProjectionRuleDeclaration projectionRule)
        {
            var hash = pipelineContext.GetStateEntry<Hash>(HashStateKey);
            if (hash == null)
            {
                hash = PrepareHash(input);
                pipelineContext.AddStateEntry(HashStateKey, hash);
            }

            var templateFile = _fileProvider.GetFile(pipelineContext, projection, projectionRule);
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

        private static Hash PrepareHash(ProjectionInput generatorContext)
        {
            Hash hash = null;
            if (generatorContext == null)
            {
                hash = new Hash();
                return hash;
            }

            var modelContainer = generatorContext.Model;

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

            foreach (var p in GetParameters(generatorContext))
            {
                if (!hash.ContainsKey(p.Key))
                {
                    hash.Add(p.Key, p.Value);
                }
            }

            return hash;
        }

        private static IEnumerable<KeyValuePair<string, string>> GetParameters(ProjectionInput generatorContext)
        {
            var allParameters = new Dictionary<string, string>();

            foreach (var parameter in generatorContext.Parameters)
            {
                if (allParameters.ContainsKey(parameter.Key))
                {
                    allParameters[parameter.Key] = parameter.Value;
                }
                else
                {
                    allParameters.Add(parameter.Key, parameter.Value);
                }
            }

            foreach (var parameter in generatorContext.ModelContext.Parameters)
            {
                if (allParameters.ContainsKey(parameter.Key))
                {
                    allParameters[parameter.Key] = parameter.Value;
                }
                else
                {
                    allParameters.Add(parameter.Key, parameter.Value);
                }
            }

            return allParameters;
        }

        public string TransformString(PipelineContext targetContext, ProjectionInput input, string outputPathRule)
        {
            var hash = PrepareHash(input);
            var template = Template.Parse(outputPathRule);
            var output = template.Render(hash);
            return output;
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
