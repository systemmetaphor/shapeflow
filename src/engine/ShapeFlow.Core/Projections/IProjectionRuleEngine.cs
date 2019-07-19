using System.Collections.Generic;
using System.Threading.Tasks;
using ShapeFlow.Declaration;
using ShapeFlow.Shapes;

namespace ShapeFlow.Projections
{
    /// <summary>
    /// A component that takes an input shape and returns an output shape transformed
    /// according to the projection rule function.
    /// </summary>
    public interface IProjectionRuleEngine
    {
        /// <summary>
        /// Gets the programming language of the projection rule.
        /// </summary>
        string RuleLanguage { get; }

        /// <summary>
        /// Gets a glob the allows finding the projection rules by convention.
        /// </summary>
        string RuleSearchExpression { get; }

        /// <summary>
        /// Gets the input shape formats that the engine is able to process.
        /// </summary>
        IEnumerable<ShapeFormat> InputFormats { get; }

        /// <summary>
        /// Gets the output shape formats that the engine is able to return.
        /// </summary>
        IEnumerable<ShapeFormat> OutputFormats { get; }

        /// <summary>
        /// Executes the transformation of the given shape using the given projection rule and parameters.
        /// </summary>
        /// <param name="inputShape">The shape to transform.</param>
        /// <param name="projectionRule">The rule to use in the transformation.</param>
        /// <param name="parameters">The parameters to pass to the rule.</param>
        /// <returns>The resulting shape.</returns>
        Task<Shape> Transform(Shape inputShape, ProjectionRule projectionRule, IDictionary<string, string> parameters);
    }
}
