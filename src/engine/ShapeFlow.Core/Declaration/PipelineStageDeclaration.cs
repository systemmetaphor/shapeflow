using System;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class PipelineStageDeclaration
    {
        public string Name { get; private set; }

        public string Selector { get; private set; }
        
        public string ProjectionRef { get; private set; }
        
        public static PipelineStageDeclaration Parse(JObject pipelineObject, string stageName)
        {
            var selector = pipelineObject.GetStringPropertyValue("selector");
            var projectionRef = pipelineObject.GetStringPropertyValue("projectionRef");
            
            var pipeline = new PipelineStageDeclaration
            {
                Name = stageName,
                ProjectionRef = projectionRef,
                Selector = selector
            };

            return pipeline;
        }
    }
}