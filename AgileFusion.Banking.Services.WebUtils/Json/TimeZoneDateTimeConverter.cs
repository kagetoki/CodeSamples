using Newtonsoft.Json.Converters;
using System;
using Newtonsoft.Json;
using IDS.Time;

namespace AgileFusion.Banking.Services.WebUtils.Json
{
    public class TimeZoneDateTimeConverter : IsoDateTimeConverter
    {
        public const string DATETIME_FORMAT = "yyyy-MM-ddThh:mm:ss";
        public const string DATETIME_TIMEZONE_FORMAT = "yyyy-MM-ddThh:mm:sszzz";
        public TimeZoneInfo ServerTimeZone { get; set; }
        public TimeZoneDateTimeConverter()
        {
            ServerTimeZone = TimeZoneHelper.EnterpriseTimeZone;
            DateTimeFormat = DATETIME_FORMAT;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var date = value as DateTime?;
            if (!date.HasValue)
            {
                return;
            }
            if(date.Value.Kind != DateTimeKind.Unspecified)
            {
                DateTime datetime;
                if(date.Value.Kind == DateTimeKind.Utc)
                {
                    datetime = TimeZoneInfo.ConvertTimeFromUtc(date.Value, ServerTimeZone);
                }
                else
                {
                    var temp = TimeZoneInfo.ConvertTimeToUtc(date.Value);
                    datetime = TimeZoneInfo.ConvertTimeFromUtc(temp, ServerTimeZone);
                }
                DateTimeOffset dt = new DateTimeOffset(datetime, ServerTimeZone.BaseUtcOffset);
                writer.WriteValue(dt.ToString(DATETIME_TIMEZONE_FORMAT));
                return;
            }
            base.WriteJson(writer, value, serializer);
        }
    }
}
