using ShapeFlow.Declaration;

namespace ShapeFlow.Projections
{
    public interface ITextTemplateEngine
    {
        string TemplateLanguage { get; }

        ModelToTextOutputFile Transform(
            PipelineContext pipelineContext, 
            ProjectionInput input,  
            ProjectionDeclaration projection,
            ProjectionRuleDeclaration projectionRule);

        string TransformString(PipelineContext generatorContext, ProjectionInput input, string inputText);
    }
}
