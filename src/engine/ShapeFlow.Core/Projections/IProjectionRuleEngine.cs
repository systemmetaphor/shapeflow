using System.Collections.Generic;
using System.Threading.Tasks;
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

        Task<ProjectionContext> Transform(ProjectionContext projectionContext, ProjectionRuleDeclaration projectionRule);

        Task<string> TransformString(ProjectionContext projectionContext, string inputText);
    }
}
