using System;
using System.Collections.Generic;
using System.Linq;
using ShapeFlow.Declaration;
using ShapeFlow.Projections;

namespace ShapeFlow
{
    public class SolutionEventContext
    {
        public SolutionEventContext(Solution solution)
        {
            Solution = solution ?? throw new ArgumentNullException(nameof(solution));
        }

        public SolutionEventContext(SolutionEventContext previous)
            : this(previous, Enumerable.Empty<PipelineContext>())
        {
        }

        public SolutionEventContext(SolutionEventContext previous, IEnumerable<PipelineContext> projections)
        {
            Solution = previous.Solution ?? throw new ArgumentNullException(nameof(previous));
            Projections = projections;
        }

        public Solution Solution { get; }

        public IEnumerable<PipelineContext> Projections { get; }
    }
}
