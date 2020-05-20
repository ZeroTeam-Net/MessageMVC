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
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            try
            {
                if (reader.Value is string str)
                    return Enum.Parse(objectType, str);
            }
            catch
            {
            }
            return existingValue;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }
    }
}