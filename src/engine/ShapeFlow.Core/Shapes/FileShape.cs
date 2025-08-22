namespace ShapeFlow.Shapes
{
    public class FileShape : Shape
    {
        public FileShape(string text, string path)
            : base(ShapeFormat.None, path)
        {            
            Text = text;
            Path = path;
        }

        public string Path { get; }

        public string Text { get; }

        public override object GetInstance()
        {
            return this;
        }
    }
}
