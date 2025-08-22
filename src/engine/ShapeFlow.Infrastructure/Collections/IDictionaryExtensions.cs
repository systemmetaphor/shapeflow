using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShapeFlow.Collections
{
    public static class IDictionaryExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if(dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source)
        {
            foreach (var value in source)
            {
                target.AddOrUpdate(value.Key, value.Value);
            }
        }

        public static void WriteAsObjectLiteral(this IDictionary<string, string> value, JsonWriter writer)
        {
            writer.WriteStartObject();

            foreach (var parameter in value)
            {
                writer.WritePropertyName(parameter.Key);
                writer.WriteValue(parameter.Value);
            }

            writer.WriteEndObject();
        }
    }
}
