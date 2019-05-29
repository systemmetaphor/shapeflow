using System.Collections.Generic;
using ShapeFlow.Infrastructure;
using Newtonsoft.Json.Linq;

namespace ShapeFlow.ModelDriven
{
    public class PipelineDeclaration
    {
        public PipelineDeclaration(string name, string selector, InputDeclaration input, TransformationDeclaration transformation, OutputDeclaration ouputer)
        {
            Name = name;
            Transformation = transformation;
            Selector = selector;
            Output = ouputer;
            Input = input;
        }

        public string Name { get; }

        public string Selector { get; }

        public TransformationDeclaration Transformation { get; }

        public OutputDeclaration Output { get; }

        public InputDeclaration Input { get; } 

        public static PipelineDeclaration Parse(JObject pipelineObject)
        {
            var pipelineName = pipelineObject.GetStringPropertyValue("name");
            var selectorText = pipelineObject.GetStringPropertyValue("selector");
            var transformationObject = pipelineObject.GetValue("transformation") as JObject;
            var inputObject = pipelineObject.GetValue("input") as JObject;
            var outputerObject = pipelineObject.GetValue("output") as JObject;
            
            var transformation = TransformationDeclaration.Parse(transformationObject);
            var output = OutputDeclaration.Parse(outputerObject);
            var input = InputDeclaration.Parse(inputObject);

            var pipeline = new PipelineDeclaration(
                pipelineName,
                selectorText,
                input,
                transformation,
                output);

            return pipeline;
        }

        
    }
}