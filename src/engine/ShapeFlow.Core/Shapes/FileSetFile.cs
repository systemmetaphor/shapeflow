namespace ShapeFlow.Shapes
{
    public class FileSetFile
    {
        public FileSetFile(string text, string outputPath)
        {            
            Text = text;
            OutputPath = outputPath;
        }

        public string OutputPath { get; }

        public string Text { get; }
    }
}
