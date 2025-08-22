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
        private readonly IContainer _container;

        public ShapeFlowEngine(ShapeManager shapeManager, ProjectionRegistry projectionRegistry, IContainer container)
        {
            _shapeManager = shapeManager;
            _projectionRegistry = projectionRegistry;

            _container = container;
        }

        public async Task Run(IDictionary<string, string> parameters)
        {
            var solution = SolutionDeclaration.ParseFile(parameters);
            await Run(solution);
        }

        public async Task Run(SolutionDeclaration solutionDeclaration)
        {
            solutionDeclaration = await _projectionRegistry.Process(solutionDeclaration);
            try
            {
                var pipeline = AssemblePipeline(solutionDeclaration);
                await pipeline.PublishAll();
            }
            catch (Exception e)
            {
                AppTrace.Error(e.Message);
                AppTrace.Verbose(e.ToString());
            }
        }

        public Solution AssemblePipeline(SolutionDeclaration solutionDeclaration)
        {
            var result = new Solution(solutionDeclaration, _container);

            foreach (var pipelineDeclaration in solutionDeclaration.Pipelines)
            {
                var pipeline = new Pipeline(pipelineDeclaration, _container);

                foreach (var pipelineStage in pipelineDeclaration.Stages)
                {
                    var transformationHandler = new ProjectionPipelineStageHandler(pipelineStage);
                    pipeline.AddHandler(transformationHandler);
                }

                result.Add(pipeline);
            }

            return result;
        }
    }
}
