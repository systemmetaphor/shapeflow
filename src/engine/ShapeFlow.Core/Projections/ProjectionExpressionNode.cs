namespace ShapeFlow.Projections
{
    public class ProjectionExpressionNode
    {
        public ProjectionExpressionNode(ProjectionCardinality cardinality)
            : this(string.Empty, cardinality)
        {
        }

        public ProjectionExpressionNode(string selector, ProjectionCardinality cardinality)
        {
            Selector = selector;
            Cardinality = cardinality;
        }

        public  string Selector { get; }

        public bool IsSuperSetSelector => string.IsNullOrWhiteSpace(Selector);

        public  ProjectionCardinality Cardinality { get; }
    }
}