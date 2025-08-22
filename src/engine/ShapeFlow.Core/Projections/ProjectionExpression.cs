namespace ShapeFlow.Projections
{
    public class ProjectionExpression
    {
        public ProjectionExpression(ProjectionExpressionNode source, ProjectionOperator op, ProjectionExpressionNode target)
        {
            Source = source;
            Target = target;
            Operator = op;
        }

        public  ProjectionExpressionNode Source { get; }

        public  ProjectionOperator Operator { get; }

        public  ProjectionExpressionNode Target { get; }
    }
}