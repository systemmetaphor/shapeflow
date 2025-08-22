using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Pipelines
{
    public class Pipeline : IDisposable
    {
        private readonly IContainer _container;
        private readonly List<PipelineStageHandler> _handlers;

        public Pipeline(PipelineDeclaration pipelineDeclaration, IContainer container)
        {
            PipelineDeclaration = pipelineDeclaration;
            _container = container;
            _handlers = new List<PipelineStageHandler>();
        }

        public PipelineDeclaration PipelineDeclaration { get; }

        public void AddHandler(PipelineStageHandler what)
        {
            what.Parent = this;
            _handlers.Add(what);
        }

        public async Task Publish(ShapeContext context)
        {
            foreach (var handler in _handlers)
            {
                await handler.OnNext(context);
            }
        }

        public void Dispose()
        {
            _handlers.Clear();
        }

        internal T GetService<T>() where T : class
        {
            return _container.Resolve<T>();
        }
    }
}
