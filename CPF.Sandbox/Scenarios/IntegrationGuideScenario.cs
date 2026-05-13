using CPF.Service.SendDataToMongoDB.Model.Order;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.DTO;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Interfaces;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Models;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serialization;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.BLL;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Services;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// SDK QuickStart 教學 — 客戶端 5 分鐘接入示範。
    /// 完整文件: docs/SDK_QuickStart.md
    ///
    /// 範例 1: 註冊查詢服務 + 呼叫 SearchByBuyerAsync
    /// 範例 2: 註冊 Mongo 寫入 Repository + 呼叫 UpdateData ($set + $unset + $push 一次合併)
    /// </summary>
    public static class IntegrationGuideScenario
    {
        public static Task RunAsync()
        {
            Console.WriteLine();
            Console.WriteLine("=== dbSDK QuickStart 教學 ===");

            Demo1_RegisterAndQuery();
            Demo2_RegisterAndUpdate();

            Console.WriteLine();
            Console.WriteLine("=== 教學完成、完整步驟請見 docs/SDK_QuickStart.md ===");
            return Task.CompletedTask;
        }

        // ─────────────────────────────────────────────────────────────────
        // 範例 1: 註冊查詢服務 + 呼叫查詢
        // ─────────────────────────────────────────────────────────────────
        private static void Demo1_RegisterAndQuery()
        {
            Console.WriteLine();
            Console.WriteLine("【範例 1】註冊查詢服務 + 呼叫 SearchByBuyerAsync");
            Console.WriteLine();

            // 啟動初始化 (Program.cs 開頭跑一次)
            MongoSerializationConfig.Register();
            MongoMap.EnsureClassMapsRegistered();

            // DI 註冊
            var services = new ServiceCollection();
            services.AddSingleton<IElasticOrderSearchService>(BuildSearchService);

            // 注入並使用
            using var sp = services.BuildServiceProvider();
            var sdk = sp.GetRequiredService<IElasticOrderSearchService>();

            var req = new SearchOrderInfoByBuyerIdModel
            {
                MemSid = 12345,
                PageIndex = 1,
                PageSize = 20
            };

            Console.WriteLine($"  服務型別: {sdk.GetType().Name}");
            Console.WriteLine($"  呼叫: var result = await sdk.SearchByBuyerAsync(req);");
            Console.WriteLine($"        req.MemSid={req.MemSid}, PageIndex={req.PageIndex}, PageSize={req.PageSize}");
        }

        private static IElasticOrderSearchService BuildSearchService(IServiceProvider _)
        {
            const string esEndpoint = "http://localhost:9200";
            const string mongoUri = "mongodb://root:example@localhost:27017";
            const string mongoDbName = "CpfOrderDb";

            var conn = new ConnectionSettings { Elastic = new DbDetail { EndPoint = esEndpoint } };
            var esDriver = new ElasticDriver("Elastic", conn);
            var esMap = new ElasticMap();
            var esRepo = new ElasticRepository<OrderDocument>(esDriver, esMap, "orders-*");
            var esDal = new OrderSearchDal(esRepo);

            var mongoMap = new MongoMap();
            var mongoClient = new MongoClient(mongoUri);
            var db = mongoClient.GetDatabase(mongoDbName);
            var orderColl = db.GetCollection<MongoOrder>("Orders");
            var userColl = db.GetCollection<MongoUser>("Users");
            var mongoDal = new MongoSearchDal(orderColl, userColl, mongoMap);

            var bll = new ElasticOrderSearchBll(esDal, mongoDal, null, null);
            return new ElasticOrderSearchService(bll, null);
        }

        // ─────────────────────────────────────────────────────────────────
        // 範例 2: 註冊 Mongo 寫入 Repository + 呼叫更新 ($set + $unset + $push)
        // ─────────────────────────────────────────────────────────────────
        private static void Demo2_RegisterAndUpdate()
        {
            Console.WriteLine();
            Console.WriteLine("【範例 2】註冊 Mongo Repository + 呼叫 UpdateData ($set + $unset + $push 一次合併)");
            Console.WriteLine();

            // DI 註冊
            var services = new ServiceCollection();
            services.AddSingleton<MongoDBDriver>(_ =>
            {
                var settings = new ConnectionSettings
                {
                    Mongo = new DbDetail
                    {
                        User = "root",
                        Password = "example",
                        Uri = "cluster0.example.mongodb.net/CpfOrderDb"
                    }
                };
                return new MongoDBDriver("MongoDB", settings);
            });
            services.AddSingleton<MongoMap>();
            services.AddSingleton<IDTO, DTO>();
            services.AddSingleton<IMongoDBRepository<OrderModel>>(sp =>
                new MongoRepository<OrderModel>(
                    sp.GetRequiredService<MongoDBDriver>(),
                    sp.GetRequiredService<MongoMap>(),
                    sp.GetRequiredService<IDTO>(),
                    "Order"));

            // 注入並使用
            using var sp = services.BuildServiceProvider();
            var repo = sp.GetRequiredService<IMongoDBRepository<OrderModel>>();

            string filter = "{ \"coom_no\": \"CM2604160395986\" }";

            var patch = new OrderModel
            {
                PK = "CM2604160395986",
                C_Order_M = new C_Order_M_Model
                {
                    CoomStatus = "30",
                    CoomSellerMemo = "已寄出"
                }
            };

            var options = new MongoUpdateOptions
            {
                IsUpsert = false,
                UnsetFields = new List<string> { "c_order_m.coom_cancel_reason" },
                PushFields = new Dictionary<string, BsonValue>
                {
                    ["e_shipment_l"] = new BsonDocument
                    {
                        ["esml_esmm_status"] = "10",
                        ["esml_status_datetime"] = DateTime.UtcNow
                    }
                }
            };

            // dry-run 預覽 (不執行 I/O、看實際送 Mongo 的 BSON 指令)
            // 用 SDK public static helper FlattenBsonDocument 自己拼 — 與 UpdateData 內部邏輯一致
            var rawPatch = patch.ToBsonDocument();
            var flatSet = MongoRepository<OrderModel>.FlattenBsonDocument(rawPatch);

            var cmd = new BsonDocument { ["$set"] = flatSet };
            if (options.UnsetFields?.Count > 0)
            {
                var unset = new BsonDocument();
                foreach (var f in options.UnsetFields) unset.Add(f, "");
                cmd["$unset"] = unset;
            }
            if (options.PushFields?.Count > 0)
            {
                var push = new BsonDocument();
                foreach (var p in options.PushFields)
                    push.Add(p.Key, new BsonDocument("$each", new BsonArray { p.Value }));
                cmd["$push"] = push;
            }

            var pretty = new JsonWriterSettings { Indent = true };
            Console.WriteLine($"  filter: {filter}");
            Console.WriteLine("  update:");
            Console.WriteLine(cmd.ToJson(pretty));
            Console.WriteLine();
            Console.WriteLine("  生產呼叫: var result = await repo.UpdateData(filter, patch, options);");
        }
    }
}
