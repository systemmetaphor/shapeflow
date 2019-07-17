using ShapeFlow.Declaration;

namespace ShapeFlow.Projections
{
    public interface ITextTemplateEngine
    {
        string TemplateLanguage { get; }

        string TemplateSearchExpression { get; }

        FileSetFile Transform(ProjectionContext projectionContext, ProjectionRuleDeclaration projectionRule);

        string TransformString(ProjectionContext projectionContext, string inputText);
    }
}
