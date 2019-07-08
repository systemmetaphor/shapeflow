using System;
using System.Collections.Generic;
using System.Text;
using ShapeFlow.PackageManagement;

namespace ShapeFlow.ModelDriven
{
    public abstract class ShapeFlowExtension
    {
        public abstract Package Package { get; }
    }
}
