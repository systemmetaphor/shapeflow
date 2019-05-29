namespace ShapeFlow.ModelDriven
{
    public interface ITextTemplateEngine
    {
        string TemplateLanguage { get; }

        ModelToTextOutputFile Transform(ProjectionContext transformationContext, ProjectionInput input,  TransformationRuleDeclaration tranformationRule);

        string TransformString(ProjectionContext generatorContext, ProjectionInput input, string inputText);
    }
}
