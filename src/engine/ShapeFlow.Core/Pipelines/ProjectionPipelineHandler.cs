using System;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Output;
using ShapeFlow.Projections;

namespace ShapeFlow.Pipelines
{
    public class ProjectionPipelineHandler : PipelineHandler
    {
        private readonly PipelineDeclaration _pipelineDeclaration;

        public ProjectionPipelineHandler(PipelineDeclaration pipelineDeclaration) : base(pipelineDeclaration)
        {
            _pipelineDeclaration = pipelineDeclaration;
        }

        
        public string Selector => _pipelineDeclaration.Input.Selector;

        protected override async Task Process(ShapeContext context)
        {
            var projectionEngine = Parent.GetService<ProjectionEngine>();
            var fileService = Parent.GetService<IFileService>();
            
            var projectionContext = new ProjectionContext(Parent.Solution, PipelineDeclaration, context);
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
