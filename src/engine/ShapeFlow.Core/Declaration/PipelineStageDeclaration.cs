using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class PipelineStageDeclaration
    {
        public string Name { get; private set; }

        public string Selector { get; private set; }
        
        public string ProjectionRef { get; private set; }

        public static PipelineStageDeclaration Create(string stageName, string projectionRef, string selector)
        {
            return new PipelineStageDeclaration
            {
                Name = stageName,
                ProjectionRef =  projectionRef,
                Selector = selector
            };
        }
        
        public static PipelineStageDeclaration Parse(JObject pipelineObject, string stageName)
        {
            var selector = pipelineObject.GetStringPropertyValue("selector");
            var projectionRef = pipelineObject.GetStringPropertyValue("projectionRef");

            return Create(stageName, projectionRef, selector);
        }

        public static void WriteTo(JsonTextWriter writer, PipelineStageDeclaration value)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(ProjectionRef).ToCamelCase());
            writer.WriteValue(value.ProjectionRef);
            
            writer.WritePropertyName(nameof(Selector).ToCamelCase());
            writer.WriteValue(value.Selector);

            writer.WriteEndObject();
        }
    }
}