using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Serialization
{
    // 對齊 Golden Recipe (Search_*_*.txt) 樣張時間格式：
    //   "coomCreateDatetime": "2026-05-05T20:54:00.773"
    //   無 Z 後綴 + 含毫秒 .fff + 已轉成 Asia/Taipei local time
    //
    // ES 出口為 UTC (Kind=Utc)，本 converter 寫入時轉 Asia/Taipei；
    // Kind=Unspecified 時假定為已是台北時間，直接格式化（避免重複轉換造成偏移 8 小時）。
    public sealed class DateTimeNoZConverter : JsonConverter<DateTime?>
    {
        private const string Format = "yyyy-MM-ddTHH:mm:ss.fff";

        private static readonly TimeZoneInfo TaipeiTz = ResolveTaipeiTz();

        private static TimeZoneInfo ResolveTaipeiTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei"); }
            catch (TimeZoneNotFoundException) { }
            try { return TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"); }
            catch (TimeZoneNotFoundException) { }
            return TimeZoneInfo.CreateCustomTimeZone("Asia/Taipei", TimeSpan.FromHours(8), "Asia/Taipei", "Asia/Taipei");
        }

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrEmpty(s)) return null;
                if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
                    return dt;
            }
            return reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value is null) { writer.WriteNullValue(); return; }
            var dt = value.Value;
            var local = dt.Kind switch
            {
                DateTimeKind.Utc => TimeZoneInfo.ConvertTimeFromUtc(dt, TaipeiTz),
                DateTimeKind.Local => TimeZoneInfo.ConvertTime(dt, TaipeiTz),
                _ => dt,
            };
            writer.WriteStringValue(local.ToString(Format, CultureInfo.InvariantCulture));
        }
    }
}
