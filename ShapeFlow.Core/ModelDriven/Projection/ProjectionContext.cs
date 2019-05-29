﻿using System.Collections.Generic;
using ShapeFlow.Infrastructure;
using ShapeFlow.ModelDriven;

namespace ShapeFlow.ModelDriven
{
    public class ProjectionContext
    {
        private readonly Dictionary<string, string> _parameters;
        private readonly Dictionary<string, object> _stateEntries;

        public ProjectionContext(
            Solution solution, 
           PipelineDeclaration pipelineDeclaration)
        {
            _parameters = new Dictionary<string, string>();
            _stateEntries = new Dictionary<string, object>();

            Solution = solution;
            Pipeline = pipelineDeclaration;
            
            //Generator = generator;
        }

        public Solution Solution { get; private set; }

        public PipelineDeclaration Pipeline { get; }

        public TransformationDeclaration Transformation => Pipeline?.Transformation;

        public InputDeclaration Input => Pipeline?.Input;

        public string GeneratorName => Transformation?.GeneratorName;

        //public GeneratorDeclaration Generator { get; }

        public ModelToTextOutput Output { get; set; }

        public IEnumerable<KeyValuePair<string, string>> Parameters => _parameters;
               
        public string GetParameter(string name)
        {
            if(_parameters?.ContainsKey(name) ?? false)
            {
                return _parameters[name];
            }
            
            return null;
        }

        public void SetParameter(string name, string value)
        {
            if (_parameters?.ContainsKey(name) ?? false)
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
