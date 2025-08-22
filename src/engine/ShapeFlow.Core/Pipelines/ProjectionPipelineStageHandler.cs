using System;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Output;
using ShapeFlow.Projections;

namespace ShapeFlow.Pipelines
{
    public class ProjectionPipelineStageHandler : PipelineStageHandler
    {
        private readonly PipelineStageDeclaration _pipelineStageDeclaration;

        public ProjectionPipelineStageHandler(PipelineStageDeclaration pipelineStageDeclaration) : base(pipelineStageDeclaration)
        {
            _pipelineStageDeclaration = pipelineStageDeclaration;
        }

        
        public string Selector => _pipelineStageDeclaration.Selector;

        protected override async Task Process(ShapeContext shapeContext)
        {
            var projectionEngine = Parent.GetService<ProjectionEngine>();
            var fileService = Parent.GetService<IFileService>();
            
            var projectionContext = new ProjectionContext(PipelineDeclaration, PipelineStageDeclaration, shapeContext);
            projectionContext = await projectionEngine.Transform(projectionContext);


            fileService.Process(projectionContext);
        }

        protected override Task<bool> ShouldProcess(ShapeContext context)
        {
            // naif implementation of filter
             return Task.FromResult(Selector.Equals(context.Shape.Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
