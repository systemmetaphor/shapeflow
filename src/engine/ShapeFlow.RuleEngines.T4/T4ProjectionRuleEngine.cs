using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TextTemplating;
using Mono.TextTemplating;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;
using ShapeFlow.Projections;
using ShapeFlow.Shapes;

namespace ShapeFlow.RuleEngines.T4
{
    public class T4ProjectionRuleEngine : IProjectionRuleEngine
    {   
        public T4ProjectionRuleEngine()
        {
            RuleLanguage = RuleLanguages.T4;
            RuleSearchExpression = ".\\**\\*.tt";
        }

        public string RuleLanguage { get; }

        public string RuleSearchExpression { get; }

        public IEnumerable<ShapeFormat> InputFormats { get; } = new[] {
            ShapeFormat.Clr,
            ShapeFormat.Json,
            ShapeFormat.Xml,
            ShapeFormat.Yaml
        };

        public IEnumerable<ShapeFormat> OutputFormats { get; } = new[] {
            ShapeFormat.FileSet
        };

        public Task<Shape> Transform(Shape inputShape, ProjectionRule projectionRule,
            IReadOnlyDictionary<string, string> parameters)
        {   
            var outputText = TransformCore(inputShape.GetInstance(), parameters, projectionRule.Text);

            Shape resultingShape = new StringShape(outputText);
            return Task.FromResult(resultingShape);
        }

        private string TransformCore(object model, IReadOnlyDictionary<string, string> parameters, string templateFileText)
        {
            var generator = new ToolTemplateGenerator();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(assembly.Location))
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
                GetType().Assembly.FullName);

            var pt = ParsedTemplate.FromText(templateFileText, generator);

            var symbols = new List<string>();

            foreach (var directive in pt.Directives)
            {
                if (!directive.Name.Equals("property"))
                {
                    continue;
                }

                if (directive.Attributes.TryGetValue("name", out string symbol))
                {
                    symbols.Add(symbol);
                }
            }

            

            PrepareInput(model, parameters, symbols);

            string outputPath = null;
            var outputText = generator.ProcessTemplate(pt, string.Empty, templateFileText, ref outputPath);

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

        private static void PrepareInput(object model, IReadOnlyDictionary<string, string> parameters, IEnumerable<string> detectedSymbols)
        {
            
            var inputs = new Dictionary<string, object>();

            

            var unwrapModel = false;

            if (model is JObject modelJObject)
            {
                model = modelJObject.ToDictionary();
            }

            if (model is IDictionary<string, object> modelDictionary)
            {
                if (modelDictionary.Keys.Intersect(detectedSymbols).Any())
                {
                    unwrapModel = true;

                    foreach (var kvp in modelDictionary)
                    {
                        inputs.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            // this flag tells the system if the template is expecting properties
            // matching the model instance keys / properties or if it is expecting
            // the model itself
            if (!unwrapModel)
            {
                inputs.Add("model", model);
            }

            foreach (var p in parameters)
            {
                inputs.Add(p.Key, p.Value);
            }

            foreach (var i in inputs)
            {
                CallContext.SetData(i.Key, i.Value);
            }
        }
    }

    class ToolTemplateGenerator : TemplateGenerator
    {
        public ToolTemplateGenerator()
        {
            Refs.Add(typeof(CompilerErrorCollection).Assembly.Location);
        }

        protected override ITextTemplatingSession CreateSession() => new ToolTemplateSession(this);

        public string PreprocessTemplate(
            ParsedTemplate pt,
            string inputFile,
            string inputContent,
            string className,
            TemplateSettings settings = null)
        {
            TemplateFile = inputFile;
            string classNamespace = null;
            int s = className.LastIndexOf('.');
            if (s > 0)
            {
                classNamespace = className.Substring(0, s);
                className = className.Substring(s + 1);
            }

            return Engine.PreprocessTemplate(pt, inputContent, this, className, classNamespace, out var language, out var references, settings);
        }

        public string ProcessTemplate(
            ParsedTemplate pt,
            string inputFile,
            string inputContent,
            ref string outputFile,
            TemplateSettings settings = null)
        {
            TemplateFile = inputFile;
            OutputFile = outputFile;
            using (var compiled = Engine.CompileTemplate(pt, inputContent, this, settings))
            {
                var result = compiled?.Process();
                outputFile = OutputFile;
                return result;
            }
        }
    }

    class ToolTemplateSession : ITextTemplatingSession
    {
        readonly Dictionary<string, object> session = new Dictionary<string, object>();

        readonly ToolTemplateGenerator toolTemplateGenerator;

        public ToolTemplateSession(ToolTemplateGenerator toolTemplateGenerator)
        {
            this.toolTemplateGenerator = toolTemplateGenerator;
        }

        public object this[string key]
        {
            get => session[key];
            set => session[key] = value;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public ICollection<string> Keys => session.Keys;
        public ICollection<object> Values => session.Values;
        public int Count => session.Count;
        public bool IsReadOnly => false;
        public void Add(string key, object value) => session.Add(key, value);
        public void Add(KeyValuePair<string, object> item) => session.Add(item.Key, item.Value);
        public void Clear() => session.Clear();
        public bool Contains(KeyValuePair<string, object> item) => session.TryGetValue(item.Key, out object v) && item.Value == v;
        public bool ContainsKey(string key) => session.ContainsKey(key);
        public bool Remove(string key) => session.Remove(key);
        public bool Remove(KeyValuePair<string, object> item) => Contains(item) && session.Remove(item.Key);
        public bool Equals(ITextTemplatingSession other) => other != null && Id == other.Id;
        public bool Equals(Guid other) => Id.Equals(other);
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => session.GetEnumerator();
        public bool TryGetValue(string key, out object value) => session.TryGetValue(key, out value);
        IEnumerator IEnumerable.GetEnumerator() => session.GetEnumerator();

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var v in session)
            {
                array[arrayIndex++] = v;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException();
        }

    }
}
