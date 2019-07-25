using System;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Projections;

namespace ShapeFlow.Pipelines
{
    public abstract class PipelineHandler
    {
        protected PipelineHandler(PipelineStageDeclaration pipelineStageDeclaration)
        {
            PipelineStageDeclaration = pipelineStageDeclaration;
        }

        public PipelineStageDeclaration PipelineStageDeclaration
        {
            get;
        }

        public string Name => PipelineStageDeclaration.Name;

        internal SolutionPipeline Parent { get; set; }

        public void OnCompleted()
        {            
        }

        public void OnError(Exception error)
        {            
        }

        public async Task OnNext(ShapeContext shapeContext)
        {
            if(await ShouldProcess(shapeContext))
            {
                await Process(shapeContext);
            }
        }

        protected abstract Task Process(ShapeContext context);

        protected abstract Task<bool> ShouldProcess(ShapeContext context);
    }
}
