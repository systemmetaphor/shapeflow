using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;

namespace ShapeFlow.Loaders.KriativityReflectedModel
{
    public static class ReflectionRuleSet
    {
        public static bool ShouldIncludeAsStateObject(this TypeDef t)
        {
            return t.Name.EndsWith("State");
        }

        public static bool ShouldIncludeAsDataObject(this TypeDef t)
        {
            return t.Name.EndsWith("Data");
        }

        public static bool ShouldIncludeAsEventObject(this TypeDef t)
        {
            return t.Name.EndsWith("Event");
        }
    }
}
