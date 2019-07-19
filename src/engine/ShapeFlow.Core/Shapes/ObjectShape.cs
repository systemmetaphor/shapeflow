namespace ShapeFlow.Shapes
{
    public class ObjectShape : Shape
    {
        private readonly object _model;

        public ObjectShape(object model)
            : base(ShapeFormat.Clr, string.Empty)
        {
            _model = model;
        }

        public override object GetInstance()
        {
            return _model;
        }
    }
}