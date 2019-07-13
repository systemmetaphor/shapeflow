using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
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

            if(transformationObject == null)
            {
                return null;
            }

            var transformation = TransformationDeclaration.Parse(transformationObject);

            if (outputerObject == null)
            {
                return null;
            }

            var output = OutputDeclaration.Parse(outputerObject);

            if (inputObject == null)
            {
                return null;
            }

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