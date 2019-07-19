using System;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Projections;

namespace ShapeFlow.Pipelines
{
    public abstract class PipelineHandler
    {
        protected PipelineHandler(PipelineDeclaration pipelineDeclaration)
        {
            PipelineDeclaration = pipelineDeclaration;
        }

        public PipelineDeclaration PipelineDeclaration
        {
            get;
        }

        public string Name => PipelineDeclaration.Name;

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
