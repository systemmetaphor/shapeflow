using System;
using ShapeFlow.Declaration;
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

        protected override void ProcessShape(ShapeContext context)
        {
            var projectionEngine = Parent.GetService<ProjectionEngine>();
            var fileService = Parent.GetService<IFileService>();
            
            var pipelineContext = new ProjectionContext(Parent.Solution, PipelineDeclaration, context);
            var output = projectionEngine.Transform(pipelineContext);
            fileService.Process(pipelineContext, output);
        }

        protected override bool ShouldProcess(ShapeContext context)
        {
            // naif implementation of filter
            return Selector.Equals(context.Model.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
