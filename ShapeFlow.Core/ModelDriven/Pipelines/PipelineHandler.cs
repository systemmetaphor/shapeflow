using System;

namespace ShapeFlow.Pipelines
{
    public abstract class PipelineHandler : IObserver<ShapeContext>
    {
        public abstract string Name { get; }

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
                ProcessShape(shapeContext);
            }
        }

        protected abstract void ProcessShape(ShapeContext context);

        protected abstract bool ShouldProcess(ShapeContext context);

    }
}
