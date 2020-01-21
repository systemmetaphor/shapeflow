using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeFlow.Loaders.KriativityReflectedModel
{
    public static class CSharpSyntax 
    {
        public static string ConvertClrTypeToKeyword(Type t)
        {
            var actualType = t;
            var isNullable = IsNullableType(t);            
            if(isNullable)
            {
                actualType = Nullable.GetUnderlyingType(t);
            }

            if(typeof(System.Boolean) == actualType)
            {
                return string.Concat("bool", isNullable ? "?" : string.Empty);
            }

            if(typeof(System.Int16) == actualType)
            {
                return string.Concat("short", isNullable ? "?" : string.Empty);
            }

            if (typeof(System.Int32) == actualType)
            {
                return string.Concat("int", isNullable ? "?" : string.Empty);
            }
            
            if (typeof(System.Int64) == actualType)
            {
                return string.Concat("long", isNullable ? "?" : string.Empty);
            }

            if (typeof(System.Single) == actualType)
            {
                return string.Concat("float", isNullable ? "?" : string.Empty);
            }

            if (typeof(System.Double) == actualType)
            {
                return string.Concat("double", isNullable ? "?" : string.Empty);
            }

            if (typeof(System.Decimal) == actualType)
            {
                return string.Concat("decimal", isNullable ? "?" : string.Empty);
            }

            if (typeof(System.DateTime) == actualType)
            {
                return string.Concat("DateTime", isNullable ? "?" : string.Empty);
            }

            if (typeof(System.String) == actualType)
            {
                return "string";
            }
            
            if(t.IsGenericType && !isNullable)
            {
                return GetFullName(t);
            }


            if(!isNullable && !string.IsNullOrWhiteSpace(actualType.FullName))
            {
                return actualType.FullName;
            }

            return string.Concat(actualType.Name, isNullable ? "?" : string.Empty);
        }

        public static bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static string GetFullName(Type t)
        {
            if (!t.IsGenericType)
            {
                if(!string.IsNullOrEmpty(t.FullName))
                {
                    return t.FullName;
                }

                return t.Name;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(t.Name.Substring(0, t.Name.LastIndexOf("`")));
            sb.Append(t.GetGenericArguments().Aggregate("<",(aggregate, type) => aggregate + (aggregate == "<" ? "" : ",") + GetFullName(type)));
            sb.Append(">");

            return sb.ToString();
        }

        public static string FormatJsonNetType(Type currentType)
        {
            var stringType = String.Join(",", currentType.AssemblyQualifiedName.Split(',').Take(2).ToArray());

            var genericArguments = currentType.GetGenericArguments();
            if (genericArguments != null && genericArguments.Length > 0)
            {
                var genericParameterConstraints = genericArguments[0].GetGenericParameterConstraints();
                if (genericParameterConstraints != null && genericParameterConstraints.Length > 0)
                {
                    var splitResult = currentType.AssemblyQualifiedName.Split(',');

                    stringType = String.Format("{0}[[{1}]],{2}", splitResult[0], String.Join(",", genericParameterConstraints[0].AssemblyQualifiedName.Split(',').Take(2).ToArray()), splitResult[1]);
                }
            }

            return stringType;
        }
    }
}
