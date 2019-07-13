using System;
using System.Collections.Generic;
using System.Linq;

namespace ShapeFlow.Infrastructure
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Determine whether a type is simple (String, Decimal, DateTime, etc) 
        /// or complex (i.e. custom class with public properties and methods).
        /// </summary>
        /// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
        public static bool IsSimpleType(
            this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new Type[] {
                typeof(String),
                typeof(Decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        public static bool IsCollectionType(this Type type)
        {
            if (type.IsGenericType)
            {
                var td = type.GetGenericTypeDefinition();
                if (td == typeof(IEnumerable<>))
                {
                    return true;
                }

                if(td == typeof(IList<>))
                {
                    return true;
                }

                if(td == typeof(List<>))
                {
                    return true;
                }

                if(td == typeof(HashSet<>))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetCollectionElementType(this Type type, out Type elementType)
        {
            elementType = null;
            var isCollectionType = type.IsCollectionType();
            if(isCollectionType)
            {
                elementType = type.GetGenericArguments()[0];
            }

            return isCollectionType;
        }
    }
}
