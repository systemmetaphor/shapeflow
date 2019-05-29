namespace ShapeFlow.ModelDriven
{
    public interface ITemplateEngineProvider
    {
        ITextTemplateEngine GetEngine(string language);
    }
}
