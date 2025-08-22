using ShapeFlow.Shapes;

namespace ShapeFlow.RuleEngines.Roslyn
{
    public class RoslynShapeFlowGlobals
    {
        public RoslynShapeFlowGlobals(Shape inputShape)
        {
            InputShape = inputShape;
            Model = inputShape.GetInstance();
        }

        public Shape InputShape { get; }
    
        public object Model { get; }
    }
}