using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A custom <see cref="JsonConverter"/> that enables <c>Newtonsoft.Json</c> to correctly serialize and deserialize
    /// Unity's <see cref="Color"/> struct as a JSON object.
    /// </summary>
    public class ColorConverter : JsonConverter<Color>
    {
        /// <summary>
        /// Writes the <see cref="Color"/> value as a JSON object with 'r', 'g', 'b', and 'a' properties.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The <see cref="Color"/> to serialize.</param>
        /// <param name="serializer">The calling <see cref="JsonSerializer"/>.</param>
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            JObject obj = new JObject
            {
                { "r", value.r },
                { "g", value.g },
                { "b", value.b },
                { "a", value.a }
            };
            obj.WriteTo(writer);
        }

        /// <summary>
        /// Reads a JSON object and converts it into a <see cref="Color"/> struct.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="hasExistingValue">A boolean indicating whether <c>existingValue</c> is not null.</param>
        /// <param name="serializer">The calling <see cref="JsonSerializer"/>.</param>
        /// <returns>The deserialized <see cref="Color"/> from the JSON object.</returns>
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            float r = (float)obj["r"];
            float g = (float)obj["g"];
            float b = (float)obj["b"];
            float a = (float)obj["a"];
            return new Color(r, g, b, a);
        }
    }
}