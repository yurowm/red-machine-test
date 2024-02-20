using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Utils.Jsons
{
    public static class JsonConverters
    {
        public static List<JsonConverter> GetAllConverters()
        {
            var result = new List<JsonConverter>
            {
                new Vector3Converter(),
                new Vector2Converter(),
                new ColorConverter(),
            };

            return result;
        }

        public class Vector3Converter : JsonConverter<Vector3>
        {
            public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("x");
                writer.WriteValue(value.x);

                writer.WritePropertyName("y");
                writer.WriteValue(value.y);

                writer.WritePropertyName("z");
                writer.WriteValue(value.z);

                writer.WriteEndObject();
            }

            public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue,
                bool hasExistingValue, JsonSerializer serializer)
            {
                var result = new Vector3();

                reader.Read();
                reader.Read();
                result.x = Convert.ToSingle(reader.Value);

                reader.Read();
                reader.Read();
                result.y = Convert.ToSingle(reader.Value);

                reader.Read();
                reader.Read();
                result.z = Convert.ToSingle(reader.Value);

                reader.Read();

                return result;
            }
        }

        public class Vector2Converter : JsonConverter<Vector2>
        {
            public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("x");
                writer.WriteValue(value.x);

                writer.WritePropertyName("y");
                writer.WriteValue(value.y);

                writer.WriteEndObject();
            }

            public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue,
                bool hasExistingValue, JsonSerializer serializer)
            {
                var result = new Vector2();

                reader.Read();
                reader.Read();
                result.x = Convert.ToSingle(reader.Value);

                reader.Read();
                reader.Read();
                result.y = Convert.ToSingle(reader.Value);

                reader.Read();
                
                return result;
            }
        }

        public class ColorConverter : JsonConverter<Color>
        {
            public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
            {
                var rgba = ColorUtility.ToHtmlStringRGBA(value);
                writer.WriteValue(rgba);
            }

            public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue,
                bool hasExistingValue, JsonSerializer serializer)
            {
                var rgba = $"#{reader.Value}";

                if (!ColorUtility.TryParseHtmlString(rgba, out var color))
                {
                    color = Color.white;
                }

                return color;
            }
        }
        
    }
}