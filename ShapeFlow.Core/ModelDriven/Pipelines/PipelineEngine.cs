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
        private readonly ModelToTextProjectionEngine _engine;

        public PipelineEngine(ModelToTextProjectionEngine engine)
        {           
            _engine = engine;         
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
