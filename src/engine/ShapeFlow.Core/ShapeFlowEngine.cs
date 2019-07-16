using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Pipelines;
using ShapeFlow.Projections;
using ShapeFlow.Shapes;

namespace ShapeFlow
{
    public class ShapeFlowEngine
    {
        private readonly ShapeManager _shapeManager;
        private readonly ProjectionRegistry _projectionRegistry;
        private readonly Dictionary<Solution, SolutionPipeline> _solutionPipelines;
        private readonly IContainer _container;

        public ShapeFlowEngine(ShapeManager shapeManager, ProjectionRegistry projectionRegistry, IContainer container)
        {
            _shapeManager = shapeManager;
            _projectionRegistry = projectionRegistry;
            _solutionPipelines = new Dictionary<Solution, SolutionPipeline>();
            _container = container;
        }

        public async Task Run(IDictionary<string, string> parameters)
        {
            var solution = Solution.ParseFile(parameters);
            await Run(solution);
        }

        public async Task Run(Solution solution)
        {
            solution = await _projectionRegistry.Process(solution);
            try
            {
                GetOrAssemblePipeline(solution).PublishAll();
            }
            catch (Exception e)
            {
                AppTrace.Error(e.Message);
                AppTrace.Verbose(e.ToString());
            }
            
            ClosePipeline(solution);
        }

        private void ClosePipeline(Solution solution)
        {
            _solutionPipelines.Remove(solution);
        }

        private SolutionPipeline GetOrAssemblePipeline(Solution solution)
        {
            if (_solutionPipelines.TryGetValue(solution, out var solutionPipeline))
            {
                return solutionPipeline;
            }

            solutionPipeline = new SolutionPipeline(solution, _container);

            foreach (var pipeline in solution.Pipelines)
            {
                _projectionRegistry.TryGet(pipeline.ProjectionRef.ProjectionName,
                    out ProjectionDeclaration projectionDecl);
                pipeline.Projection = projectionDecl;

                var transformationHandler = new ProjectionPipelineHandler(pipeline);
                solutionPipeline.AddHandler(transformationHandler);
            }

            _solutionPipelines.Add(solution, solutionPipeline);

            return solutionPipeline;
        }
    }
}
