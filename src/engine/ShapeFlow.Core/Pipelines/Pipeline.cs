using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Infrastructure;
using ShapeFlow.Shapes;

namespace ShapeFlow.Pipelines
{
    public class SolutionPipeline : IDisposable
    {
        private readonly IContainer _container;
        private readonly List<PipelineHandler> _handlers;

        public SolutionPipeline(Solution solution, IContainer container)
        {
            Solution = solution;
            _container = container;
            _handlers = new List<PipelineHandler>();
        }

        public Solution Solution { get; }

        public void AddHandler(PipelineHandler what)
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

        public async Task PublishAll()
        {
            var shapeManager = GetService<ShapeManager>();

            foreach (var shapeDeclaration in Solution.Shapes)
            {
                var shape = await shapeManager.GetOrLoad(shapeDeclaration);
                await Publish(shape);
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
