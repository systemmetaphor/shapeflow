﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ShapeFlow.Infrastructure
{
    public static class JObjectExtensions
    {
        public static IDictionary<string, object> ToDictionary(this JObject json)
        {
            var propertyValuePairs = json.ToObject<Dictionary<string, object>>();
            ProcessJObjectProperties(propertyValuePairs);
            ProcessJArrayProperties(propertyValuePairs);
            return propertyValuePairs;
        }

        public static object[] ToArray(this JArray array)
        {
            return array.ToObject<object[]>().Select(ProcessArrayEntry).ToArray();
        }

        public static string GetStringPropertyValue(this JObject o, string propertyName)
        {
            return GetStringPropertyValue(o, propertyName, string.Empty);
        }

        public static string GetStringPropertyValue(this JObject o, string propertyName, string defaultValue)
        {
            if(o == null) throw new ArgumentNullException(nameof(o));
            return o.Property(propertyName)?.Value.ToString() ?? defaultValue;
        }

        private static object ProcessArrayEntry(object value)
        {
            if (value is JObject)
            {
                return ToDictionary((JObject)value);
            }
            if (value is JArray)
            {
                return ToArray((JArray)value);
            }
            return value;
        }

        private static void ProcessJObjectProperties(IDictionary<string, object> propertyValuePairs)
        {
            var objectPropertyNames = (from property in propertyValuePairs
                                       let propertyName = property.Key
                                       let value = property.Value
                                       where value is JObject
                                       select propertyName).ToList();

            objectPropertyNames.ForEach(propertyName => propertyValuePairs[propertyName] = ToDictionary((JObject)propertyValuePairs[propertyName]));
        }

        private static void ProcessJArrayProperties(IDictionary<string, object> propertyValuePairs)
        {
            var arrayPropertyNames = (from property in propertyValuePairs
                                      let propertyName = property.Key
                                      let value = property.Value
                                      where value is JArray
                                      select propertyName).ToList();

            arrayPropertyNames.ForEach(propertyName => propertyValuePairs[propertyName] = ToArray((JArray)propertyValuePairs[propertyName]));
        }

    }
}
