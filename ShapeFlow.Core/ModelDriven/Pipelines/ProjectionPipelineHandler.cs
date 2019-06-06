using System;

namespace ShapeFlow.Pipelines
{
    public class ProjectionPipelineHandler : PipelineHandler
    {
        private readonly ProjectionContext _projectionContext;
        private readonly ModelToTextProjectionEngine _modelToTextProjectionEngine;
        private readonly IFileService _fileService;

        public ProjectionPipelineHandler(
            ProjectionContext projectionContex, 
            ModelToTextProjectionEngine modelToTextProjectionEngine,
            IFileService fileService)
        {
            _projectionContext = projectionContex;
            _modelToTextProjectionEngine = modelToTextProjectionEngine;
            _fileService = fileService;
        }

        public override string Name => _projectionContext.Pipeline.Name;

        protected override void ProcessShape(ShapeContext context)
        {
            var output = _modelToTextProjectionEngine.Transform(_projectionContext, new ProjectionInput(context));
            _fileService.Process(_projectionContext, output);
        }

        protected override bool ShouldProcess(ShapeContext context)
        {
            // naif implementation of filter
            return _projectionContext.Input.Selector.Equals(context.Model.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
