using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeFlow.Loaders.KriativityReflectedModel
{
    public static class TypeScriptSyntax
    {
        public static Type GetCollectionElementType(Type what)
        {
            if(!IsCollectionType(what))
            {
                return null;
            }

            return what.GetGenericArguments()[0];
        }

        public static bool IsCollectionType(Type what)
        {
            return IsGenericObservableCollectionType(what) || IsGenericCollectionType(what) || IsGenericListType(what) || IsGenericEnumerableType(what);
        }

        public static string ConvertClrTypeToDomainTypeScriptType(Type what, bool forExtends = false)
        {
            if(what == null)
            {
                return string.Empty;
            }

            if(what.ShouldIncludeAsDataObject())
            {
                var theName = what.Name;
                if (theName.EndsWith("Data"))
                {
                    theName = string.Concat(theName.Substring(0, theName.Length - "Data".Length), "Model");
                }

                return theName;
            }

            var collectionElementType = GetCollectionElementType(what);
            if (collectionElementType != null)
            {
                var collectionElementTypeName = CSharpSyntax.ConvertClrTypeToKeyword(collectionElementType);
                var typescriptElementTypeName = "any";

                switch(collectionElementTypeName)
                {
                    case "string":
                    case "bool":
                    case "int":
                    case "double":
                    case "long":
                    case "float":
                    case "short":
                    case "decimal":
                    case "DateTime":
                    case "System.Object":
                    case "System.TimeSpan":
                    case "System.String[]":
                        typescriptElementTypeName = ConvertClrTypeToDtoTypeScriptType(collectionElementType);
                        break;
                    default:
                        typescriptElementTypeName = ConvertClrTypeToDomainTypeScriptType(collectionElementType);
                        break;
                }

                if (forExtends)
                {
                    return string.Format("Array<{0}>", typescriptElementTypeName);
                }
                else
                {
                    return string.Format("KnockoutObservableArray<{0}>", typescriptElementTypeName);
                }
            }

            var csharpTypeName = CSharpSyntax.ConvertClrTypeToKeyword(what);
            
            switch(csharpTypeName)
            {
                case "string": return "KnockoutObservable<string>";
                case "bool": return "KnockoutObservable<boolean>";
                case "int": return "KnockoutObservable<number>";
                case "double": return "KnockoutObservable<number>";
                case "long": return "KnockoutObservable<string>";
                case "float": return "KnockoutObservable<number>";
                case "short": return "KnockoutObservable<number>";
                case "decimal": return "KnockoutObservable<number>";
                case "DateTime": return "KnockoutObservable<string>";
                case "System.Object": return "KnockoutObservable<any>";
                case "System.TimeSpan": return "KnockoutObservable<string>";
                case "System.String[]": return "KnockoutObservableArray<string>";                                
            }

            var isNullable = CSharpSyntax.IsNullableType(what);
            if (isNullable)
            {
                return ConvertClrTypeToDomainTypeScriptType(Nullable.GetUnderlyingType(what));
            }
            
            return "any";
        }

        public static string ConvertClrTypeToDtoTypeScriptType(Type what)
        {
            if (what == null)
            {
                return string.Empty;
            }

            if (what.ShouldIncludeAsDataObject())
            {
                return what.Name;
            }

            var collectionElementType = GetCollectionElementType(what);
            if (collectionElementType != null)
            {   
                return string.Format("Array<{0}>", ConvertClrTypeToDtoTypeScriptType(collectionElementType));                
            }

            var csharpTypeName = CSharpSyntax.ConvertClrTypeToKeyword(what);

            switch (csharpTypeName)
            {
                case "string": return "string";
                case "bool": return "boolean";
                case "int": return "number";
                case "double": return "number";
                case "long": return "string";
                case "float": return "number";
                case "short": return "number";
                case "decimal": return "number";
                case "DateTime": return "string";
                case "System.Object": return "any";
                case "System.TimeSpan": return "string";
                case "System.String[]": return "string";
            }

            var isNullable = CSharpSyntax.IsNullableType(what);
            if (isNullable)
            {
                return ConvertClrTypeToDtoTypeScriptType(Nullable.GetUnderlyingType(what));
            }

            return "any";
        }

        public static string GetDomainDefaultValue(Type what, bool forExtends = false)
        {
            if (what == null)
            {
                return string.Empty;
            }

            if (what.ShouldIncludeAsDataObject())
            {
                var theName = what.Name;
                if (theName.EndsWith("Data"))
                {
                    theName = string.Concat(theName.Substring(0, theName.Length - "Data".Length), "Model");
                }

                return $"new {theName}()"; ;
            }

            if (IsCollectionType(what))
            {
                Type collectionElementType = what.GetGenericArguments()[0];
                var collectionElementTypeName = CSharpSyntax.ConvertClrTypeToKeyword(collectionElementType);
                var typescriptElementTypeName = "any";

                switch (collectionElementTypeName)
                {
                    case "string":
                    case "bool":
                    case "int":
                    case "double":
                    case "long":
                    case "float":
                    case "short":
                    case "decimal":
                    case "DateTime":
                    case "System.Object":
                    case "System.TimeSpan":
                    case "System.String[]":
                        typescriptElementTypeName = ConvertClrTypeToDtoTypeScriptType(collectionElementType);
                        break;
                    default:
                        typescriptElementTypeName = ConvertClrTypeToDomainTypeScriptType(collectionElementType);
                        break;
                }

                return string.Format("ko.observableArray<{0}>([])", typescriptElementTypeName);
                
            }

            var csharpTypeName = CSharpSyntax.ConvertClrTypeToKeyword(what);

            switch (csharpTypeName)
            {
                case "string": return "ko.observable(\"\")";
                case "bool": return "ko.observable(false)";
                case "int": return "ko.observable(0)";
                case "double": return "ko.observable(0)";
                case "long": return "ko.observable(\"0\")";

                case "float": return "ko.observable(0)";
                case "short": return "ko.observable(0)";
                case "decimal": return "ko.observable(0)";
                case "DateTime": return "ko.observable<string>(moment().format(MomentFormats.DateFilterFormat))";
                case "System.Object": return "ko.observable<any>()";
                case "System.TimeSpan": return "ko.observable<string>()";
                case "System.String[]": return "ko.observableArray<string>()";
            }

            var isNullable = CSharpSyntax.IsNullableType(what);
            if (isNullable)
            {
                return GetDomainDefaultValue(Nullable.GetUnderlyingType(what));
            }

            return "null";
        }

        public static string GetDtoDefaultValue(Type what, bool forExtends = false)
        {
            if (what == null)
            {
                return string.Empty;
            }

            if (what.ShouldIncludeAsDataObject())
            {
                return $"new {what.Name}()"; ;
            }

            if (IsCollectionType(what))
            {
                return string.Format("[]", ConvertClrTypeToDomainTypeScriptType(what.GetGenericArguments()[0]));

            }

            var csharpTypeName = CSharpSyntax.ConvertClrTypeToKeyword(what);

            switch (csharpTypeName)
            {
                case "string": return "\"\"";
                case "bool": return "false";
                case "int": return "0";
                case "double": return "0";
                case "long": return "\"0\"";

                case "float": return "0";
                case "short": return "0";
                case "decimal": return "0";
                case "DateTime": return "moment().format(MomentFormats.DateFilterFormat)";
                case "System.Object": return "null";
                case "System.TimeSpan": return "\"\"";
                case "System.String[]": return "[]";
            }

            var isNullable = CSharpSyntax.IsNullableType(what);
            if (isNullable)
            {
                return GetDtoDefaultValue(Nullable.GetUnderlyingType(what));
            }

            return "null";
        }

        private static bool IsGenericObservableCollectionType(Type propertyType)
        {
            return propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().Equals(typeof(System.Collections.ObjectModel.ObservableCollection<>));
        }

        private static bool IsGenericCollectionType(Type propertyType)
        {
            return propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().Equals(typeof(ICollection<>));
        }

        private static bool IsGenericListType(Type propertyType)
        {
            return propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().Equals(typeof(IList<>));
        }

        private static bool IsGenericEnumerableType(Type propertyType)
        {
            return propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>));
        }
    }
}
