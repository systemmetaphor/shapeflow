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
using ShapeFlow.Shapes;

namespace ShapeFlow.TemplateEngines.DotLiquid
{
    public class DotLiquidProjectionRuleEngine : IProjectionRuleEngine
    {
        private static BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        private const string HashStateKey = "DOTLIQUID_HASH";

        private readonly TextTemplateProvider _fileProvider;
        private readonly IOutputLanguageInferenceService _inferenceService;


        public DotLiquidProjectionRuleEngine(TextTemplateProvider fileProvider, IOutputLanguageInferenceService inferenceService)
        {
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            _inferenceService = inferenceService ?? throw new ArgumentNullException(nameof(inferenceService));
            RuleSearchExpression = ".\\**\\*.liquid";
        }

        public string RuleLanguage => TextTemplateLanguages.DotLiquid;

        public string RuleSearchExpression { get; }

        public ProjectionContext Transform(ProjectionContext projectionContext, ProjectionRuleDeclaration projectionRule)
        {
            if (projectionContext.Output.Model.Format != ShapeFormat.FileSet)
            {
                throw new InvalidOperationException($"The language {RuleLanguage} can only output shapes of {nameof(ShapeFormat.FileSet)} format.");
            }

            if (!(projectionContext.Output.Model.GetInstance() is FileSet outputSet))
            {
                throw new InvalidOperationException($"You must set a non null {nameof(FileSet)} shape on the projection output shape.");
            }

            var hash = projectionContext.GetStateEntry<Hash>(HashStateKey);
            if (hash == null)
            {
                hash = PrepareHash(projectionContext.Input, projectionContext.Solution.Parameters);
                projectionContext.AddStateEntry(HashStateKey, hash);
            }

            var templateFile = _fileProvider.GetFile(projectionContext, projectionRule);
            try
            {
                var template = Template.Parse(templateFile.Text);

                var output = template.Render(hash);

                var templateFileName = projectionRule.FileName;
                var languageExtension = _inferenceService.InferFileExtension(output);
                var outputPath = Path.ChangeExtension(templateFileName, ".generated.txt");
                outputPath = Path.ChangeExtension(outputPath, languageExtension);

                var result = new FileSetFile(output, outputPath);
                outputSet.AddFile(result);
            }
            catch (Exception e)
            {
                AppTrace.Error("Template processing failed:", e);
                throw;
            }

            return projectionContext;
        }

        public string TransformString(ProjectionContext projectionContext, string outputPathRule)
        {
            var hash = PrepareHash(projectionContext.Input, projectionContext.Solution.Parameters);
            var template = Template.Parse(outputPathRule);

            var output = template.Render(hash);

            return output;
        }

        private static Hash PrepareHash(ShapeContext projectionInput, IDictionary<string, string> parameters)
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

            foreach (var p in parameters)
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
