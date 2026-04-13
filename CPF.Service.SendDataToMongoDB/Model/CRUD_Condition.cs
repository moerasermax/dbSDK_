using MongoDB.Bson.Serialization.Attributes;
using NO3._dbSDK_Imporve.Core.Models;
using System.Text.Json.Serialization;

namespace CPF.Service.SendDataToMongoDB.Model
{
    public class CRUD_Condition
    {
        public CRUD_Condition(string coom_no)
        {
            _coom_no = coom_no;
        }

        [BsonId] // 給 MongoDB Driver 看的 (轉成 BSON 時用)
        [JsonPropertyName("_id")] // 給 System.Text.Json 看的 (轉成 JSON 時用)
        public string _coom_no { get; }


    }
}
