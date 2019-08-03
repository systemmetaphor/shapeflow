using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShapeFlow.Collections;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class PipelineDeclaration
    {
        private readonly Dictionary<string, string> _parameters;
        private SolutionDeclaration _parent;

        private PipelineDeclaration()
        {
            _parameters = new Dictionary<string, string>();
        }

        //public IDictionary<string, string> Parameters => _parameters;

        public string Name { get; private set; }

        public IEnumerable<PipelineStageDeclaration> Stages { get; private set; }

        public static PipelineDeclaration Create(string pipelineName, IEnumerable<PipelineStageDeclaration> stages)
        {
            return new PipelineDeclaration
            {
                Name = pipelineName,
                Stages = stages
            };
        }

        public static PipelineDeclaration Parse(JObject pipelineObject, string pipelineName, SolutionDeclaration parent)
        {
            var stages = new List<PipelineStageDeclaration>();

            if (!(pipelineObject.GetValue("stages") is JObject stagesDeclaration))
            {
                throw new SolutionParsingException("The stages property is required.");
            }

            foreach (var property in stagesDeclaration.Properties())
            {
                if (!(property.Value is JObject stageObject))
                {
                    throw new SolutionParsingException("The stages property value has an invalid value.");
                }

                var pipeline = PipelineStageDeclaration.Parse(stageObject, property.Name);
                if (pipeline == null)
                {
                    continue;
                }

                stages.Add(pipeline);
            }

            if (stages.Count == 0)
            {
                throw new SolutionParsingException($"The pipeline {pipelineName} does not declare any stage. At least one stage is required.");
            }

            var result = new PipelineDeclaration
            {
                _parent =  parent,
                Stages = stages,
                Name =  pipelineName,
            };

            return result;
        }

        public void SetParameter(string name, string value)
        {
            _parameters.AddOrUpdate(name, value);
        }

        public string GetParameter(string name)
        {
            if (!_parameters.TryGetValue(name, out var result))
            {
                return _parent.GetParameter(name);
            }

            return result;
        }

        public IReadOnlyDictionary<string, string> ComputeAggregatedParameters()
        {
            var dictionary = new Dictionary<string, string>(_parameters);

            foreach (var parentParameter in _parent.Parameters)
            {
                if (!dictionary.ContainsKey(parentParameter.Key))
                {
                    dictionary.Add(parentParameter.Key, parentParameter.Value);
                }
            }

            return dictionary;
        }

        public static void WriteTo(JsonTextWriter writer, PipelineDeclaration value)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(Stages).ToCamelCase());

            writer.WriteStartObject();
            foreach (var stage in value.Stages)
            {
                writer.WritePropertyName(stage.Name.ToCamelCase());
                PipelineStageDeclaration.WriteTo(writer, stage);
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}