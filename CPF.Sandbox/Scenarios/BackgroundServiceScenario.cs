using CPF.Service.SendDataToElasticCloud.Model;
using CPF.Service.SendDataToMongoDB.Model.Order;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using System.Text.Json;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// 模組 C — 背景同步服務 (Background Sync) 教學場景。
    ///
    /// 完整資料流模擬:
    ///   [Producer]  CPF.Services.Redis.Post.Worker_ForCPF
    ///               → 觸發 AddOrderEvent
    ///               → Push 進 Redis List × 2 (Request_MongoDB / Request_Elastic 分流)
    ///   [Consumer A] CPF.Service.SendDataToMongoDB.Worker
    ///               → Pop Request_MongoDB
    ///               → 組 OrderModel → IMongoDBRepository.UpdateData
    ///   [Consumer B] CPF.Service.SendDataToElasticCloud.Worker
    ///               → Pop Request_Elastic
    ///               → 組 OrderInfoModel → IRepository&lt;OrderSummary&gt;.InsertData / UpdateData
    ///
    /// 本場景用 in-memory Queue 模擬 Redis Buffer、不真連 Redis。
    /// (RedisDriver 構造為 eager ConnectionMultiplexer.Connect、無 Redis 服務即立即 throw。)
    /// </summary>
    public static class BackgroundServiceScenario
    {
        // 模擬 Redis Buffer 的兩條 List (對齊 production 的 Request_MongoDB / Request_Elastic 分流)
        private static readonly Queue<string> _mongoQueue = new();
        private static readonly Queue<string> _elasticQueue = new();

        public static Task RunAsync(ConnectionSettings settings)
        {
            // settings 預留供未來真連 Mongo/ES 使用、本 dry-run 場景不需要 instantiate driver
            _ = settings;

            Step1_ProducerPush();
            Step2_MongoWorkerProcess();
            Step3_ElasticWorkerProcess();

            return Task.CompletedTask;
        }

        // ─────────────────────────────────────────────────────────────────
        // Step 1: Producer 端 — 模擬 CPF.Services.Redis.Post 觸發 AddOrderEvent
        //         真實情境:Worker_ForCPF.Createflow() 按 1 鍵觸發、用
        //         OrderRepository_Redis.InsertData() 把 Event Push 進 Redis List。
        // ─────────────────────────────────────────────────────────────────
        private static void Step1_ProducerPush()
        {
            Console.WriteLine();
            Console.WriteLine("  ▶ Step 1: Producer 觸發 AddOrderEvent");

            // 模擬一筆新增訂單事件 (對應 production 的 MongoDBAddOrder / ElasticAddOrder 簡化版)
            var addOrderEvent = new
            {
                Name = "AddOrderEvent",
                CoomNo = "CM2604160395999",
                CoocNo = "CC2265481053999",
                CoomStatus = "10",
                CoomCuamCid = 528672,
                CoomName = "Test Order"
            };

            var json = JsonSerializer.Serialize(addOrderEvent);

            // 真實 production:_redis.QueryDB="Request_MongoDB" → InsertData(MongoDBAddOrder)
            _mongoQueue.Enqueue(json);
            Console.WriteLine($"    Push → Redis List \"Request_MongoDB\"");

            // 真實 production:_redis.QueryDB="Request_Elastic" → InsertData(ElasticAddOrder)
            _elasticQueue.Enqueue(json);
            Console.WriteLine($"    Push → Redis List \"Request_Elastic\"");
            Console.WriteLine();
            Console.WriteLine($"    Event payload: {json}");
        }

        // ─────────────────────────────────────────────────────────────────
        // Step 2: Consumer A — 模擬 CPF.Service.SendDataToMongoDB.Worker
        //         真實情境:ExecuteAsync while loop 每 1 秒 GetData("Request_MongoDB"),
        //         拿到 DataJson 後 switch (query.Name) 分派、寫 Mongo。
        // ─────────────────────────────────────────────────────────────────
        private static void Step2_MongoWorkerProcess()
        {
            Console.WriteLine();
            Console.WriteLine("  ▶ Step 2: Mongo Worker 處理 Request_MongoDB");

            // 真實 production:result = await _Redis.GetData("Request_MongoDB") as Result;
            if (!_mongoQueue.TryDequeue(out var json))
            {
                Console.WriteLine("    Queue 空、跳過");
                return;
            }
            Console.WriteLine($"    Pop ← Redis List \"Request_MongoDB\"");

            // 真實 production: query = JsonSerializer.Deserialize<MongoDBAddOrder>(json);
            //                  switch (query.Name) { case "AddOrderEvent": 組 OrderModel + 寫 Mongo }
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var coomNo = root.GetProperty("CoomNo").GetString()!;

            // 組 patch (示範用簡化版、production 會塞滿 16 個欄位)
            var patch = new OrderModel
            {
                PK = coomNo,
                CoocNo = root.GetProperty("CoocNo").GetString(),
                C_Order_M = new C_Order_M_Model
                {
                    CoomStatus = root.GetProperty("CoomStatus").GetString(),
                    CoomCuamCid = root.GetProperty("CoomCuamCid").GetInt32(),
                    CoomName = root.GetProperty("CoomName").GetString()
                }
            };

            // 印實際送 Mongo 的 BSON 指令 (不執行 I/O、走 SDK public static FlattenBsonDocument)
            var flat = MongoRepository<OrderModel>.FlattenBsonDocument(patch.ToBsonDocument());
            var cmd = new BsonDocument { ["$set"] = flat };

            Console.WriteLine($"    交付 IMongoDBRepository<OrderModel>.UpdateData:");
            Console.WriteLine($"      filter: {{ \"coom_no\": \"{coomNo}\" }}");
            Console.WriteLine($"      update: {cmd.ToJson(new JsonWriterSettings { Indent = true })}");
        }

        // ─────────────────────────────────────────────────────────────────
        // Step 3: Consumer B — 模擬 CPF.Service.SendDataToElasticCloud.Worker
        //         真實情境:ExecuteAsync while loop 每 1 秒 GetData("Request_Elastic"),
        //         拿到 DataJson 後 Deserialize<ElasticAddOrder> → 組 OrderInfoModel → 寫 ES。
        // ─────────────────────────────────────────────────────────────────
        private static void Step3_ElasticWorkerProcess()
        {
            Console.WriteLine();
            Console.WriteLine("  ▶ Step 3: ES Worker 處理 Request_Elastic");

            if (!_elasticQueue.TryDequeue(out var json))
            {
                Console.WriteLine("    Queue 空、跳過");
                return;
            }
            Console.WriteLine($"    Pop ← Redis List \"Request_Elastic\"");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var coomNo = root.GetProperty("CoomNo").GetString()!;

            // 真實 production: data = new OrderInfoModel { CoomNo=..., CoomStatus=..., ... };
            var esData = new OrderInfoModel
            {
                CoomNo = coomNo,
                CoomName = root.GetProperty("CoomName").GetString(),
                CoomStatus = root.GetProperty("CoomStatus").GetString(),
                CoomCuamCid = root.GetProperty("CoomCuamCid").GetInt32()
            };

            // 印 ES Document 預覽 (snake_case 對齊 [JsonPropertyName] attribute)
            var esJson = JsonSerializer.Serialize(esData, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            Console.WriteLine($"    交付 IRepository<OrderSummary>.InsertData / UpdateData:");
            Console.WriteLine($"      Index   : orders-{DateTime.UtcNow:yyyyMM}");
            Console.WriteLine($"      Document: {esJson}");
        }
    }
}
