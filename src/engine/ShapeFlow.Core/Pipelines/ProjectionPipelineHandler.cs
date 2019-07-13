using System;
using ShapeFlow.Projections;

namespace ShapeFlow.Pipelines
{
    public class ProjectionPipelineHandler : PipelineHandler
    {
        private readonly PipelineContext _pipelineContext;
        private readonly ModelToTextProjectionEngine _modelToTextProjectionEngine;
        private readonly IFileService _fileService;

        public ProjectionPipelineHandler(
            PipelineContext pipelineContext, 
            ModelToTextProjectionEngine modelToTextProjectionEngine,
            IFileService fileService)
        {
            _pipelineContext = pipelineContext;
            _modelToTextProjectionEngine = modelToTextProjectionEngine;
            _fileService = fileService;
        }

        public override string Name => _pipelineContext.Pipeline.Name;

        protected override void ProcessShape(ShapeContext context)
        {
            var output = _modelToTextProjectionEngine.Transform(_pipelineContext, new ProjectionInput(context));
            _fileService.Process(_pipelineContext, output);
        }

        protected override bool ShouldProcess(ShapeContext context)
        {
            // naif implementation of filter
            return _pipelineContext.Input.Selector.Equals(context.Model.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
