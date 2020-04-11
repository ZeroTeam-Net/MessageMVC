using System;

namespace Newtonsoft.Json
{
    /// <summary>
    ///     枚举序列化器
    /// </summary>
    public class JsonEnumConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return Enum.Parse(objectType, existingValue?.ToString() ?? "0");
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }
    }
}