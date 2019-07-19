using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotLiquid;
using DotLiquid.NamingConventions;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.Projections;
using ShapeFlow.Shapes;

namespace ShapeFlow.RuleEngines.DotLiquid
{
    public class DotLiquidProjectionRuleEngine : IProjectionRuleEngine
    {
        private static readonly BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        public DotLiquidProjectionRuleEngine()
        {
            RuleSearchExpression = ".\\**\\*.liquid";
        }

        public string RuleLanguage => RuleLanguages.DotLiquid;

        public string RuleSearchExpression { get; }

        public IEnumerable<ShapeFormat> InputFormats { get; } = new[] {
            ShapeFormat.Clr,
            ShapeFormat.Json
        };

        public IEnumerable<ShapeFormat> OutputFormats { get; } = new[] {
            ShapeFormat.FileSet
        };

        public Task<Shape> Transform(Shape inputShape, ProjectionRule projectionRule,
            IDictionary<string, string> parameters)
        {
            string output;
            try
            {
                var template = Template.Parse(projectionRule.Text);

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
                
                var hash = PrepareHash(inputShape, parameters, symbols);
                output = template.Render(hash);
            }
            catch (Exception e)
            {
                AppTrace.Error("Template processing failed:", e);
                throw;
            }

            Shape resultingShape = new StringShape(output);
            return Task.FromResult(resultingShape);
        }

        private static Hash PrepareHash(Shape inputShape, IDictionary<string, string> parameters, IEnumerable<string> detectedSymbols)
        {
            var model = inputShape.GetInstance();
            Hash hash = null;
            if (model == null)
            {
                hash = new Hash();
                return hash;
            }
            
            if (inputShape.Format == ShapeFormat.Clr)
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
