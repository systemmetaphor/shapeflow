using System;
using ShapeFlow.Declaration;
using ShapeFlow.Projections;

namespace ShapeFlow.Pipelines
{
    public abstract class PipelineHandler : IObserver<ShapeContext>
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

        public void OnNext(ShapeContext shapeContext)
        {
            if(ShouldProcess(shapeContext))
            {
                Process(shapeContext);
            }
        }

        protected abstract void Process(ShapeContext context);

        protected abstract bool ShouldProcess(ShapeContext context);
    }
}
