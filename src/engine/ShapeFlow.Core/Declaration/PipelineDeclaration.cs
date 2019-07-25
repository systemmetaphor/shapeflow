using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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

        public string PipelineName { get; private set; }

        public IEnumerable<PipelineStageDeclaration> Stages { get; private set; }

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
                PipelineName =  pipelineName,
            };

            return result;
        }

        public void SetParameter(string name, string value)
        {
            if (_parameters.ContainsKey(name))
            {
                _parameters[name] = value;
            }
            else
            {
                _parameters.Add(name, value);
            }
        }

        public string GetParameter(string name)
        {
            if (_parameters?.ContainsKey(name) ?? false)
            {
                return _parameters[name];
            }
            
            return _parent.GetParameter(name);
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
    }
}