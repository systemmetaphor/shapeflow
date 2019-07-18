using ShapeFlow.Declaration;

namespace ShapeFlow.Projections
{
    public interface IProjectionRuleEngine
    {
        string RuleLanguage { get; }

        string RuleSearchExpression { get; }

        ProjectionContext Transform(ProjectionContext projectionContext, ProjectionRuleDeclaration projectionRule);

        string TransformString(ProjectionContext projectionContext, string inputText);
    }
}
