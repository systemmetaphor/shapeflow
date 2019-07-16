using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class PipelineDeclaration
    {
        public PipelineDeclaration(string name, string selector, InputDeclaration input, ProjectionRefDeclaration projectionRef, OutputDeclaration output)
        {
            Name = name;
            ProjectionRef = projectionRef;
            Selector = selector;
            Output = output;
            Input = input;
        }

        public string Name { get; }

        public string Selector { get; }

        public ProjectionRefDeclaration ProjectionRef { get; }

        public OutputDeclaration Output { get; }

        public InputDeclaration Input { get; } 

        public  ProjectionDeclaration Projection { get; internal set; }
        
        public static PipelineDeclaration Parse(JObject pipelineObject)
        {
            var pipelineName = pipelineObject.GetStringPropertyValue("name");
            var selectorText = pipelineObject.GetStringPropertyValue("selector");
            var transformationObject = pipelineObject.GetValue("projectionRef") as JObject;
            var inputObject = pipelineObject.GetValue("input") as JObject;
            var outputObject = pipelineObject.GetValue("output") as JObject;

            if(transformationObject == null)
            {
                return null;
            }

            var transformation = ProjectionRefDeclaration.Parse(transformationObject);

            if (outputObject == null)
            {
                return null;
            }

            var output = OutputDeclaration.Parse(outputObject);

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