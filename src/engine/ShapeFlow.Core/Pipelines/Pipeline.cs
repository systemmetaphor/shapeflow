using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Projections;
using ShapeFlow.Shapes;

namespace ShapeFlow.Pipelines
{
    public class SolutionPipeline : IDisposable
    {
        private readonly IContainer _container;
        private readonly Subject<ShapeContext> _shapeStream;
        private readonly Dictionary<PipelineHandler, IDisposable> _handlers;

        public SolutionPipeline(Solution solution, IContainer container)
        {
            Solution = solution;
            _container = container;
            _shapeStream = new Subject<ShapeContext>();
            _handlers = new Dictionary<PipelineHandler, IDisposable>();
        }

        public Solution Solution { get; }

        protected ISubject<ShapeContext> ShapeStream => _shapeStream;

        public void AddHandler(PipelineHandler what)
        {
            var subscription = _shapeStream.Subscribe(what);
            _handlers.Add(what, subscription);
            what.Parent = this;
        }

        public void Publish(ShapeContext context)
        {
            _shapeStream.OnNext(context);
        }

        public void PublishAll()
        {
            var shapeManager = GetService<ShapeManager>();

            foreach (var shapeDeclaration in Solution.ShapeDeclarations)
            {
                Publish(shapeManager.GetOrLoad(shapeDeclaration));
            }
        }

        public void Dispose()
        {
            _shapeStream?.Dispose();
        }

        internal T GetService<T>() where T:class
        {
            return _container.Resolve<T>();
        }
    }
}
