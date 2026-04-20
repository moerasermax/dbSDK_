using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NO3._dbSDK_Imporve.Core.Models;
using System.Text.Json.Serialization;

namespace CPF.Service.SendDataToMongoDB.Model
{
    public class CRUD_Condition_COOM
    {
        public CRUD_Condition_COOM(string coom_no)
        {
            _coom_no = coom_no;
        }

        [BsonId] // 給 MongoDB Driver 看的 (轉成 BSON 時用)
        [JsonPropertyName("_id")] // 給 System.Text.Json 看的 (轉成 JSON 時用)
        [BsonRepresentation(BsonType.ObjectId)] // 關鍵：允許 ObjectId 自動轉 string
        public string _coom_no { get; }
    }

    public class CRUD_Condition_COOC
    {
        // 這裡是用 cooc_no 當 Element，不是 PK，所以通常不會報 ObjectId 的錯誤
        // 但要注意你的屬性名稱 _coom_no 是不是寫錯了（應該是 _cooc_no 吧？）
        [BsonElement("cooc_no")]
        [BsonIgnoreIfNull]
        public string cooc_no { get; } // 建議名稱改為 _cooc_no 與參數一致

        public CRUD_Condition_COOC(string Cooc_no)
        {
            cooc_no = Cooc_no;
        }
    }
}
