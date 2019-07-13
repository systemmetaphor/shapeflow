using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace ShapeFlow.Pipelines
{
    public abstract class Pipeline
    {
        private readonly Subject<ShapeContext> _shapeStream;
        private readonly Dictionary<PipelineHandler, IDisposable> _handlers;

        protected Pipeline()
        {
            _shapeStream = new Subject<ShapeContext>();
            _handlers = new Dictionary<PipelineHandler, IDisposable>();
        }

        protected ISubject<ShapeContext> ShapeStream => _shapeStream;

        public void AddHandler(PipelineHandler what)
        {
            var subscription = _shapeStream.Subscribe(what);
            _handlers.Add(what, subscription);            
        }

        public void Publish(ShapeContext context)
        {
            _shapeStream.OnNext(context);
        }
    }
}
