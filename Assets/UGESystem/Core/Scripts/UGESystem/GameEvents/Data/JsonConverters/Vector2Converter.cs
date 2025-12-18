using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A custom <see cref="JsonConverter"/> that enables <c>Newtonsoft.Json</c> to correctly serialize and deserialize
    /// Unity's <see cref="Vector2"/> struct as a JSON object.
    /// </summary>
    public class Vector2Converter : JsonConverter<Vector2>
    {
        /// <summary>
        /// Writes the <see cref="Vector2"/> value as a JSON object with 'x' and 'y' properties.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The <see cref="Vector2"/> to serialize.</param>
        /// <param name="serializer">The calling <see cref="JsonSerializer"/>.</param>
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            JObject obj = new JObject
            {
                { "x", value.x },
                { "y", value.y }
            };
            obj.WriteTo(writer);
        }

        /// <summary>
        /// Reads a JSON object and converts it into a <see cref="Vector2"/> struct.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="hasExistingValue">A boolean indicating whether <c>existingValue</c> is not null.</param>
        /// <param name="serializer">The calling <see cref="JsonSerializer"/>.</param>
        /// <returns>The deserialized <see cref="Vector2"/> from the JSON object.</returns>
        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            float x = (float)obj["x"];
            float y = (float)obj["y"];
            return new Vector2(x, y);
        }
    }
}