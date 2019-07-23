using System;

namespace ShapeFlow.Projections
{
    public class ProjectionExpressionParser
    {
        public static ProjectionExpression Parse(string expression)
        {
            // naif parser
            if (string.Equals("map(i,f,o)", expression))
            {
                return new ProjectionExpression(new ProjectionExpressionNode(ProjectionCardinality.One), ProjectionOperator.Map, new ProjectionExpressionNode(ProjectionCardinality.One));
            }

            throw new NotSupportedException();
        }
    }
}