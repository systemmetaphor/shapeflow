namespace ShapeFlow.Projections
{
    public class ModelToTextOutputFile
    {
        public ModelToTextOutputFile(string text, string outputPath)
        {            
            Text = text;
            OutputPath = outputPath;
        }

        public string OutputPath { get; }

        public string Text { get; }
    }
}
