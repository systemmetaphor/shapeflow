using System.Collections.Generic;
using ShapeFlow.Declaration;

namespace ShapeFlow.Projections
{
    public class ProjectionContext
    {
        private readonly Dictionary<string, string> _parameters;
        private readonly Dictionary<string, object> _stateEntries;

        public ProjectionContext(PipelineDeclaration pipelineDeclaration, PipelineStageDeclaration pipelineStageDeclaration, ShapeContext shape)
        {
            _parameters = new Dictionary<string, string>();
            _stateEntries = new Dictionary<string, object>();

            PipelineDeclaration = pipelineDeclaration;
            PipelineStageDeclaration = pipelineStageDeclaration;
            Input = shape;
        }

        public PipelineDeclaration PipelineDeclaration { get; }

        public PipelineStageDeclaration PipelineStageDeclaration { get; }
        
        public ShapeContext Input { get; } 

        public ShapeContext Output { get; internal set; }
       
        public string GetParameter(string name)
        {
            if(_parameters.ContainsKey(name))
            {
                return _parameters[name];
            }
            
            return null;
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

        public void AddStateEntry(string key, object value)
        {
            if (_stateEntries.ContainsKey(key))
            {
                _stateEntries[key] = value;
            }
            else
            {
                _stateEntries.Add(key, value);
            }
        }
                        
        public T GetStateEntry<T>(string key)
        {
            _stateEntries.TryGetValue(key, out object result);
            return (T)result;
        }
    }
}
