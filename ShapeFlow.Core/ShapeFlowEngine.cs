using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Pipelines;
using ShapeFlow.Projections;
using ShapeFlow.Shapes;

namespace ShapeFlow
{
    public class ShapeFlowEngine
    {
        private readonly ModelToTextProjectionEngine _engine;
        private readonly IFileService _fileService;
        private readonly ShapeManager _shapeManager;
        private readonly ProjectionRegistry _projectionRegistry;

        public ShapeFlowEngine(
            ModelToTextProjectionEngine engine, 
            IFileService fileService,
            ShapeManager shapeManager,
            ProjectionRegistry projectionRegistry)
        {           
            _engine = engine;
            _fileService = fileService;
            _shapeManager = shapeManager;
            _projectionRegistry = projectionRegistry;
        }                

        public async Task Run(IDictionary<string, string> parameters)
        {
            var solution = Solution.ParseFile(parameters);
            await Run(new SolutionEventContext(solution));
        }

        public async Task Run(Solution solution)
        {
            await Run(new SolutionEventContext(solution));
        }

        public async Task Run(SolutionEventContext context)
        {
            context = await _projectionRegistry.Process(context);

            var transformationPipeline = new TransformationPipeline();
            foreach(var pipeline in context.Solution.Pipelines)
            {
                var projectionContext = new ProjectionContext(context.Solution, pipeline);
                var transformationHandler = new ProjectionPipelineHandler(projectionContext, _engine, _fileService);
                transformationPipeline.AddHandler(transformationHandler);
            }

            foreach (var modelDecl in context.Solution.Models)
            {
                var shapeContext = _shapeManager.GetOrLoad(modelDecl);
                transformationPipeline.Publish(shapeContext);
            }
        }
    }
}
