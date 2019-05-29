using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShapeFlow.Infrastructure;
using ShapeFlow.ModelDriven.Models;

namespace ShapeFlow.ModelDriven.Pipelines
{
    public class PipelineEngine
    {
        private readonly IModelManager _modelManager;
        private readonly ModelToTextProjectionEngine _engine;
        private readonly TextGeneratorRegistry _generatorRegistry;
        private readonly IFileService _fileService;

        public PipelineEngine(IModelManager modelManager, ModelToTextProjectionEngine engine, TextGeneratorRegistry generatorRegistry, IFileService fileService)
        {
            _modelManager = modelManager;
            _engine = engine;
            _generatorRegistry = generatorRegistry;
            _fileService = fileService;
        }

        public SolutionEventContext Process(SolutionEventContext context)
        {       
            var projections = new List<ProjectionContext>();

            foreach (var pipeline in context.Solution.Pipelines)
            {                
                projections.Add(_engine.Transform(new ProjectionContext(context.Solution, pipeline)));                
            }

            return new SolutionEventContext(context, projections);
        }
    }
}
