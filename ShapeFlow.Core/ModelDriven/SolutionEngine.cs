using ShapeFlow.Infrastructure;
using ShapeFlow.Models;
using ShapeFlow.Pipelines;

namespace ShapeFlow
{
    public class SolutionEngine
    {
        private readonly ModelManager _modelManager;
        private readonly TextGeneratorRegistry _generatorRegistry;
        private readonly IFileService _fileService;
        private readonly PipelineEngine _pipelineEngine;

        public SolutionEngine(ModelManager modelManager, PipelineEngine engine, TextGeneratorRegistry generatorRegistry, IFileService fileService)
        {
            _modelManager = modelManager;
            _pipelineEngine = engine;
            _generatorRegistry = generatorRegistry;
            _fileService = fileService;
        }

        public void Run(SolutionEventContext context)
        {            
            context = _modelManager.Process(context);
            context = _generatorRegistry.Process(context);
            context = _pipelineEngine.Process(context);
            _fileService.Process(context);
        }
    }
}
