using System;
using dnlib.DotNet;

namespace ShapeFlow.Loaders.KriativityReflectedModel
{          
    public partial class ReflectedObjectProperty
    {
        public string Name { get; set; }
              
        public TypeDef DotNetType { get; set; }
        
        public string TargetType { get; set; }

        public string TargetInitialValue { get; set; }

        public bool IsObsolete { get; set; }

        public string TargetName { get; set; }

        public bool IsBusinessObject { get; set; }

        public string BusinessObjectType { get; internal set; }

        public bool IsBusinessObjectCollection { get; set; }
        
        public string DataTransferObjectTargetInitialValue { get; internal set; }

        public string DataTransferObjectType { get; internal set; }
    }
}
