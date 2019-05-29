using ShapeFlow.Infrastructure;
using ShapeFlow.ModelDriven.Models;
using ShapeFlow.ModelDriven.Pipelines;

namespace ShapeFlow.ModelDriven
{
    public class SolutionEngine
    {
        private readonly IModelManager _modelManager;
        private readonly TextGeneratorRegistry _generatorRegistry;
        private readonly IFileService _fileService;
        private readonly PipelineEngine _pipelineEngine;

        public SolutionEngine(IModelManager modelManager, PipelineEngine engine, TextGeneratorRegistry generatorRegistry, IFileService fileService)
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
