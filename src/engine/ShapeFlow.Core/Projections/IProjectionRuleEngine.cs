using System.Collections.Generic;
using ShapeFlow.Declaration;
using ShapeFlow.Shapes;

namespace ShapeFlow.Projections
{
    public interface IProjectionRuleEngine
    {
        string RuleLanguage { get; }

        string RuleSearchExpression { get; }

        IEnumerable<ShapeFormat> InputFormats { get; }

        IEnumerable<ShapeFormat> OutputFormats { get; }

        ProjectionContext Transform(ProjectionContext projectionContext, ProjectionRuleDeclaration projectionRule);

        string TransformString(ProjectionContext projectionContext, string inputText);
    }
}
