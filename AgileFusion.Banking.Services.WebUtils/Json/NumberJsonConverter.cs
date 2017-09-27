using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AgileFusion.Banking.Services.WebUtils.Json
{
    /// <summary>
    /// Represents JSON converter that writes enumerations as a numbers.
    /// </summary>
    /// <seealso cref="StringEnumConverter" />
    public class NumberEnumConverter: StringEnumConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var enumValue = value as Enum;
            if (enumValue != null)
                writer.WriteValue(Convert.ToInt32(value));
            else
            {
                base.WriteJson(writer, value, serializer);
            }
        }
    }
}
