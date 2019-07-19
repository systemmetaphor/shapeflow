namespace ShapeFlow.Shapes
{
    public class StringShape : Shape
    {
        private readonly string _model;

        public StringShape(string model)
            : base(ShapeFormat.Text, string.Empty)
        {
            _model = model;
        }

        public override object GetInstance()
        {
            return _model;
        }
    }
}