using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Globalization;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serialization
{
    /// <summary>
    /// 自定義 MongoDB 日期序列化器
    /// 支援讀取資料庫中現存的各種語系日期字串（如「下午」格式）
    /// </summary>
    public class MongoDateTimeSerializer : SerializerBase<DateTime>, IRepresentationConfigurable
    {
        public BsonType Representation => BsonType.DateTime;

        public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.GetCurrentBsonType();

            if (bsonType == BsonType.DateTime)
            {
                var milliseconds = context.Reader.ReadDateTime();
                return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
            }
            else if (bsonType == BsonType.String)
            {
                var dateString = context.Reader.ReadString();
                if (TryParseMultiCultureDateTime(dateString, out var result))
                {
                    return result;
                }
                throw new FormatException($"無法解析日期字串: {dateString}");
            }
            else if (bsonType == BsonType.Null)
            {
                context.Reader.ReadNull();
                return DateTime.MinValue;
            }
            else
            {
                throw new NotSupportedException($"不支持將 {bsonType} 反序列化為 DateTime");
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime value)
        {
            // 寫入時統一轉為標準 BsonDateTime (Date 型別)
            context.Writer.WriteDateTime(value.Ticks / 10000 - 62135596800000);
        }

        public IBsonSerializer WithRepresentation(BsonType representation)
        {
            return this;
        }

        private static bool TryParseMultiCultureDateTime(string dateString, out DateTime result)
        {
            var formats = new[]
            {
                "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd", "yyyy/MM/dd HH:mm:ss", "yyyy/MM/dd",
                "yyyy年MM月dd日 tt hh:mm:ss", "yyyy年MM月dd日", "yyyy/MM/dd tt hh:mm:ss",
                "yyyy/MM/dd 下午 hh:mm:ss", "yyyy/MM/dd 上午 hh:mm:ss"
            };

            var cultures = new[] { CultureInfo.InvariantCulture, new CultureInfo("zh-TW"), new CultureInfo("en-US") };

            foreach (var culture in cultures)
            {
                if (DateTime.TryParse(dateString, culture, DateTimeStyles.None, out result)) return true;
            }

            var normalizedString = dateString.Replace("上午", "AM").Replace("下午", "PM");
            return DateTime.TryParse(normalizedString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }
    }

    /// <summary>
    /// 針對 Nullable<DateTime> 的序列化器
    /// </summary>
    public class MongoNullableDateTimeSerializer : SerializerBase<DateTime?>, IRepresentationConfigurable
    {
        private readonly MongoDateTimeSerializer _baseSerializer = new();

        public BsonType Representation => BsonType.DateTime;

        public override DateTime? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.GetCurrentBsonType();
            if (bsonType == BsonType.Null)
            {
                context.Reader.ReadNull();
                return null;
            }
            return _baseSerializer.Deserialize(context, args);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime? value)
        {
            if (value.HasValue)
                _baseSerializer.Serialize(context, args, value.Value);
            else
                context.Writer.WriteNull();
        }

        public IBsonSerializer WithRepresentation(BsonType representation) => this;
    }
}
