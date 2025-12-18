using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A custom <see cref="JsonConverter"/> that enables <c>Newtonsoft.Json</c> to correctly serialize and deserialize
    /// Unity's <see cref="Vector3"/> struct as a JSON object.
    /// </summary>
    public class Vector3Converter : JsonConverter<Vector3>
    {
        /// <summary>
        /// Writes the <see cref="Vector3"/> value as a JSON object with 'x', 'y', and 'z' properties.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The <see cref="Vector3"/> to serialize.</param>
        /// <param name="serializer">The calling <see cref="JsonSerializer"/>.</param>
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            JObject obj = new JObject
            {
                { "x", value.x },
                { "y", value.y },
                { "z", value.z }
            };
            obj.WriteTo(writer);
        }

        /// <summary>
        /// Reads a JSON object and converts it into a <see cref="Vector3"/> struct.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="hasExistingValue">A boolean indicating whether <c>existingValue</c> is not null.</param>
        /// <param name="serializer">The calling <see cref="JsonSerializer"/>.</param>
        /// <returns>The deserialized <see cref="Vector3"/> from the JSON object.</returns>
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            float x = (float)obj["x"];
            float y = (float)obj["y"];
            float z = (float)obj["z"];
            return new Vector3(x, y, z);
        }
    }
}