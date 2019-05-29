using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using DotLiquid;
using DotLiquid.NamingConventions;
using ShapeFlow.Infrastructure;
using Newtonsoft.Json.Linq;

namespace ShapeFlow.ModelDriven.TemplateEngines
{
    public class DotLiquidTemplateEngine : ITextTemplateEngine
    {
        private static BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        private const string HashStateKey = "DOTLIQUID_HASH";

        private readonly TextTemplateProvider _fileProvider;
        private readonly IOutputLanguageInferenceService _inferenceService;


        public DotLiquidTemplateEngine(ILoggingService loggingService, TextTemplateProvider fileProvider, IOutputLanguageInferenceService inferenceService)
        {
            LoggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            _inferenceService = inferenceService ?? throw new ArgumentNullException(nameof(inferenceService));
        }

        protected ILoggingService LoggingService { get; }

        public string TemplateLanguage => TextTemplateLanguages.DotLiquid;

        public ModelToTextOutputFile Transform(ProjectionContext transformationContext, ProjectionInput input, TransformationRuleDeclaration tranformationRule)
        {
            var hash = transformationContext.GetStateEntry<Hash>(HashStateKey);
            if (hash == null)
            {
                hash = PrepareHash(input);
                transformationContext.AddStateEntry(HashStateKey, hash);
            }

            var templateFile = _fileProvider.GetFile(transformationContext, tranformationRule);
            ModelToTextOutputFile result;
            try
            {
                var template = Template.Parse(templateFile.Text);
                var output = template.Render(hash);

                var outputPath = string.Empty;

                if (string.IsNullOrWhiteSpace(tranformationRule.OutputPathTemplate))
                {
                    var templateFileName = tranformationRule.TemplateName;
                    var languageExtension = _inferenceService.InferFileExtension(output);
                    outputPath = Path.ChangeExtension(templateFileName, languageExtension);
                }
                else
                {
                    var nameTemplate = Template.Parse(tranformationRule.OutputPathTemplate);
                    outputPath = nameTemplate.Render(hash);
                }

                result = new ModelToTextOutputFile(output, outputPath);
            }
            catch (Exception e)
            {
                LoggingService.Error("Template processing failed:", e);
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

            if (modelContainer.Format == ModelFormat.Clr)
            {
                // on CLR models we need to configure the engine to allow the public properties
                // of the model objects
                PrepareDotLiquidEngine(modelContainer.GetType());
            }

            Template.NamingConvention = new CSharpNamingConvention();

            var model = modelContainer.GetModelInstance();

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

        public string TransformString(ProjectionContext targetContext, ProjectionInput input, string outputPathRule)
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
