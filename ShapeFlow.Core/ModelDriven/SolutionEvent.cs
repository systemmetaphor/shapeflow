using System;
using System.Collections.Generic;
using System.Linq;

namespace ShapeFlow.ModelDriven
{
    public class SolutionEventContext
    {
        public SolutionEventContext(Solution solution)
        {
            Solution = solution ?? throw new ArgumentNullException(nameof(solution));
        }

        public SolutionEventContext(SolutionEventContext previous)
            : this(previous, Enumerable.Empty<ProjectionContext>())
        {
        }

        public SolutionEventContext(SolutionEventContext previous, IEnumerable<ProjectionContext> projections)
        {
            Solution = previous.Solution ?? throw new ArgumentNullException(nameof(previous));
            Projections = projections;
        }

        public Solution Solution { get; }

        public IEnumerable<ProjectionContext> Projections { get; }
    }
}
