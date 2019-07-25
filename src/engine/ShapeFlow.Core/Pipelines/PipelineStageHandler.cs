using System;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Projections;

namespace ShapeFlow.Pipelines
{
    public abstract class PipelineStageHandler
    {
        protected PipelineStageHandler(PipelineStageDeclaration pipelineStageDeclaration)
        {
            PipelineStageDeclaration = pipelineStageDeclaration;
        }

        public PipelineDeclaration PipelineDeclaration => Parent.PipelineDeclaration;

        public PipelineStageDeclaration PipelineStageDeclaration
        {
            get;
        }

        public string Name => PipelineStageDeclaration.Name;

        internal Pipeline Parent { get; set; }

        public async Task OnNext(ShapeContext shapeContext)
        {
            if(await ShouldProcess(shapeContext))
            {
                await Process(shapeContext);
            }
        }

        protected abstract Task Process(ShapeContext shapeContext);

        protected abstract Task<bool> ShouldProcess(ShapeContext context);
    }
}
