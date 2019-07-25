using System.Collections.Generic;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Shapes;

namespace ShapeFlow.Pipelines
{
    public class Solution
    {
        private readonly Dictionary<string, Pipeline> _solutionPipelines;
        private readonly IContainer _container;

        public Solution(SolutionDeclaration declaration, IContainer container)
        {
            _solutionPipelines = new Dictionary<string, Pipeline>();
            _container = container;
            Declaration = declaration;
        }

        public  SolutionDeclaration Declaration { get; }

        public void Add(Pipeline pipeline)
        {
            _solutionPipelines.Add(pipeline.PipelineDeclaration.PipelineName, pipeline);
        }

        public async Task PublishAll()
        {
            var shapeManager = GetService<ShapeManager>();

            foreach (var shapeDeclaration in Declaration.Shapes)
            {
                var shape = await shapeManager.GetOrLoad(shapeDeclaration);
                foreach (var pipeline in _solutionPipelines.Values)
                {
                    await pipeline.Publish(shape);
                }
            }
        }

        internal T GetService<T>() where T : class
        {
            return _container.Resolve<T>();
        }
    }
}