using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotLiquid;
using DotLiquid.NamingConventions;
using DotLiquid.Tags;
using Newtonsoft.Json.Linq;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Output;
using ShapeFlow.Projections;
using ShapeFlow.Shapes;

namespace ShapeFlow.RuleEngines.DotLiquid
{
    public class DotLiquidProjectionRuleEngine : IProjectionRuleEngine
    {
        private static readonly BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

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

        public IEnumerable<ShapeFormat> InputFormats { get; } = new[] {
            ShapeFormat.Clr,
            ShapeFormat.Json
        };

        public IEnumerable<ShapeFormat> OutputFormats { get; } = new[] {
            ShapeFormat.FileSet
        };

        public ProjectionContext Transform(ProjectionContext projectionContext, ProjectionRuleDeclaration projectionRule)
        {
            if (projectionContext.Output.Shape.Format != ShapeFormat.FileSet)
            {
                throw new InvalidOperationException($"The language {RuleLanguage} can only output shapes of {nameof(ShapeFormat.FileSet)} format.");
            }

            if (!(projectionContext.Output.Shape.GetInstance() is FileSet outputSet))
            {
                throw new InvalidOperationException($"You must set a non null {nameof(FileSet)} shape on the projection output shape.");
            }

            var templateFile = _fileProvider.GetFile(projectionContext, projectionRule);

            try
            {
                var template = Template.Parse(templateFile.Text);

                IEnumerable<string> symbols;

                try
                {
                    var detector = new SymbolDetector();
                    template.Render(detector);
                    symbols = detector.Symbols;
                }
                catch (Exception)
                {
                    symbols = Enumerable.Empty<string>();
                }

                var hash = projectionContext.GetStateEntry<Hash>(HashStateKey);
                if (hash == null)
                {
                    hash = PrepareHash(projectionContext.Input, projectionContext.Solution.Parameters, symbols);
                    projectionContext.AddStateEntry(HashStateKey, hash);
                }

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
            
            var template = Template.Parse(outputPathRule);

            IEnumerable<string> symbols;

            try
            {
                var detector = new SymbolDetector();
                template.Render(detector);
                symbols = detector.Symbols;
            }
            catch (Exception)
            {
                symbols = Enumerable.Empty<string>();
            }

            foreach (var symbol in symbols)
            {
                AppTrace.Information(symbol);
            }

            var hash = PrepareHash(projectionContext.Input, projectionContext.Solution.Parameters, symbols);
            var output = template.Render(hash);

            return output;
        }

        private static Hash PrepareHash(ShapeContext inputContext, IDictionary<string, string> parameters, IEnumerable<string> detectedSymbols)
        {
            Hash hash = null;
            if (inputContext == null)
            {
                hash = new Hash();
                return hash;
            }

            var shape = inputContext.Shape;
            var model = shape.GetInstance();

            if (model == null)
            {
                return new Hash();
            }

            if (shape.Format == ShapeFormat.Clr)
            {
                // on CLR models we need to configure the engine to allow the public properties
                // of the model objects
                PrepareDotLiquidEngine(model.GetType());
            }

            Template.NamingConvention = new CSharpNamingConvention();

            if (model is JObject modelJObject)
            {
                model = modelJObject.ToDictionary();
            }

            if (model is IDictionary<string, object> modelDictionary)
            {
                if (modelDictionary.Keys.Intersect(detectedSymbols).Any())
                {
                    hash = Hash.FromDictionary(modelDictionary);
                }
                else
                {
                    hash = new Hash {{ "model", model }};
                }
            }

            if (hash == null)
            {
                hash = Hash.FromAnonymousObject(model);
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
            var propertiesToConsider = rootType.GetProperties(BindingFlags);

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

    public class SymbolDetector : Hash
    {
        private readonly List<string> _symbols;

        public SymbolDetector()
        {
            _symbols = new List<string>();
        }

        public IEnumerable<string> Symbols => _symbols.AsReadOnly();

        protected override object GetValue(string key)
        {
            _symbols.Add(key);

            return base.GetValue(key);
        }
    }
}
