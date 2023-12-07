using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;

namespace ShapeFlow.Loaders.KriativityReflectedModel
{
    public static class CSharpSyntax 
    {
        public static string ConvertClrTypeToKeyword(TypeDef t)
        {
            return t.Name.String;

            //var actualType = t;

            //var isNullable = false;// IsNullableType(t);            
            ////if(isNullable)
            ////{
            ////    actualType = Nullable.GetUnderlyingType(t);
            ////}

            //if(typeof(bool).Name == actualType.Name.String)
            //{
            //    return string.Concat("bool", isNullable ? "?" : string.Empty);
            //}

            //if(typeof(System.Int16) == actualType)
            //{
            //    return string.Concat("short", isNullable ? "?" : string.Empty);
            //}

            //if (typeof(System.Int32) == actualType)
            //{
            //    return string.Concat("int", isNullable ? "?" : string.Empty);
            //}

            //if (typeof(System.Int64) == actualType)
            //{
            //    return string.Concat("long", isNullable ? "?" : string.Empty);
            //}

            //if (typeof(System.Single) == actualType)
            //{
            //    return string.Concat("float", isNullable ? "?" : string.Empty);
            //}

            //if (typeof(System.Double) == actualType)
            //{
            //    return string.Concat("double", isNullable ? "?" : string.Empty);
            //}

            //if (typeof(System.Decimal) == actualType)
            //{
            //    return string.Concat("decimal", isNullable ? "?" : string.Empty);
            //}

            //if (typeof(System.DateTime) == actualType)
            //{
            //    return string.Concat("DateTime", isNullable ? "?" : string.Empty);
            //}

            //if (typeof(System.String) == actualType)
            //{
            //    return "string";
            //}

            //if(t.GenericParameters(). && !isNullable)
            //{
            //    return GetFullName(t);
            //}


            //if(!isNullable && !string.IsNullOrWhiteSpace(actualType.FullName))
            //{
            //    return actualType.FullName;
            //}

            //return string.Concat(actualType.Name, isNullable ? "?" : string.Empty);
        }

        public static bool IsNullableType(TypeDef t)
        {
            // return t.IsGeneric() && t.GetGenericTypeDefinition() == typeof(Nullable<>);

            return false;
        }

        public static string GetFullName(TypeDef t)
        {
            if (!t.IsGeneric())
            {
                if(!string.IsNullOrEmpty(t.FullName))
                {
                    return t.FullName;
                }

                return t.Name;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(t.Name.Substring(0, t.Name.LastIndexOf("`")));
            sb.Append(t.GenericParameters.Aggregate("<",(aggregate, type) => aggregate + (aggregate == "<" ? "" : ",") + GetFullName(type.Kind.ResolveTypeDef())));
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

    public static class TypeDefExtensions
    {
        public static bool IsGeneric(this TypeDef t)
        {
            return (t.GenericParameters?.Count ?? 0) > 0;
        }
    }
}
