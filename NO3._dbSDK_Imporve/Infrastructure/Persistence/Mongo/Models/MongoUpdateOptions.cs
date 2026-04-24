using MongoDB.Bson;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Models
{
    public class MongoUpdateOptions
    {
        /// <summary>
        /// 是否走 Upsert 模式 (預設為 true)
        /// </summary>
        public bool IsUpsert { get; set; } = true;

        /// <summary>
        /// 需要強制清空的欄位清單 ($unset)
        /// </summary>
        public List<string>? UnsetFields { get; set; } = null;

        /// <summary>
        /// 更新後是否回傳新文件 (預設為 After)
        /// </summary>
        public bool ReturnNewDocument { get; set; } = true;

        /// <summary>
        /// 需要使用 $push 追加的陣列欄位
        /// Key: 陣列欄位路徑 (例如 "e_shipment_l")
        /// Value: 要追加的元素 (BsonValue)
        /// </summary>
        public Dictionary<string, BsonValue>? PushFields { get; set; } = null;

        // 未來可以輕鬆擴充，例如：
        // public string? Comment { get; set; }
        // public TimeSpan? Timeout { get; set; }
    }
}
