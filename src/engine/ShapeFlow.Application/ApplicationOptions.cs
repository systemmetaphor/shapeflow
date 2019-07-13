using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ShapeFlow
{
    public class ApplicationOptions
    {
        public ApplicationOptions()
        {            
            Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
                
        public IDictionary<string, string> Parameters { get; }
        
        public string ProjectFile { get; internal set; }                 
    }
}