using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShapeFlow.Infrastructure;

namespace ShapeFlow.Declaration
{
    public class InputDeclaration
    {
        private InputDeclaration()
        {
        }

        public string Format { get; private set; }

        public string MetaModel { get; private set; }

        public static InputDeclaration Create(string format, string metaModel)
        {
            return new InputDeclaration
            {
                Format =  format,
                MetaModel = metaModel
            };
        }

        public static InputDeclaration Parse(JObject inputObject)
        {
            var type = inputObject.GetStringPropertyValue("format");
            var metaModel = inputObject.GetStringPropertyValue("type");
            return new InputDeclaration
            {
                Format = type,
                MetaModel = metaModel 
            };
        }

        public static void WriteTo(JsonTextWriter writer, InputDeclaration value)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(Format).ToCamelCase());
            writer.WriteValue(value.Format);

            writer.WritePropertyName(nameof(MetaModel).ToCamelCase());
            writer.WriteValue(value.MetaModel);

            writer.WriteEndObject();
        }
    }

}
