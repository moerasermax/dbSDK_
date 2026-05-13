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
    /// SDK QuickStart 教學 — 對齊 docs/SDK_QuickStart.md 三段結構:
    ///   §0  全局初始化 (Global Init)
    ///   模組 A — 訂單查詢 SDK (Search)
    ///   模組 B — 貨態更新服務 (Write)
    ///
    /// 模組 A 與模組 B 各自 new ServiceCollection、完全解耦、不共享 DI 容器或任何狀態。
    /// 客戶端只用其中一個模組時、另一個模組整段可直接刪掉、不影響運作。
    /// </summary>
    public static class IntegrationGuideScenario
    {
        public static Task RunAsync()
        {
            PrintHeader("dbSDK QuickStart 教學");

            RunGlobalInit();
            RunSearchExample();
            RunUpdateExample();

            PrintHeader("教學完成、完整步驟請見 docs/SDK_QuickStart.md");
            return Task.CompletedTask;
        }

        // ═════════════════════════════════════════════════════════════════
        //  §0 全局初始化 (Global Init)
        //  Program.cs 啟動跑一次、模組 A 與 B 共用。
        // ═════════════════════════════════════════════════════════════════
        private static void RunGlobalInit()
        {
            PrintSection("§0 全局初始化 (Global Init)");

            Console.WriteLine("  兩階段靜態註冊 (Program.cs 頂部、builder.Build() 之前):");
            Console.WriteLine("    MongoSerializationConfig.Register();");
            Console.WriteLine("    MongoMap.EnsureClassMapsRegistered();");
            Console.WriteLine();

            MongoSerializationConfig.Register();
            MongoMap.EnsureClassMapsRegistered();

            Console.WriteLine("  ✅ 序列化器 + ClassMap 已註冊 (重複呼叫安全、內部有 lock + flag)");
        }

        // ═════════════════════════════════════════════════════════════════
        //  模組 A — 訂單查詢 SDK (Search)
        //  獨立 ServiceCollection、與模組 B 互不影響。
        //  適用 Search 1-7 業務查詢 (Seller / Buyer / App Dashboard)
        // ═════════════════════════════════════════════════════════════════
        private static void RunSearchExample()
        {
            PrintSection("模組 A — 訂單查詢 SDK (Search)");

            // ───── 模組 A 註冊:組 IElasticOrderSearchService ─────
            // 用到的依賴:ElasticDriver + MongoClient + ElasticRepository + MongoSearchDal + Bll + Service
            // 與模組 B「不重疊」:這裡完全不用 MongoDBDriver / IMongoDBRepository
            var services = new ServiceCollection();
            services.AddSingleton<IElasticOrderSearchService>(BuildSearchService);

            using var sp = services.BuildServiceProvider();
            var sdk = sp.GetRequiredService<IElasticOrderSearchService>();

            // ───── 呼叫範例 ─────
            var req = new SearchOrderInfoByBuyerIdModel
            {
                MemSid = 12345,
                PageIndex = 1,
                PageSize = 20
            };

            Console.WriteLine($"  服務型別: {sdk.GetType().Name}");
            Console.WriteLine($"  注入呼叫: var result = await sdk.SearchByBuyerAsync(req);");
            Console.WriteLine($"            req.MemSid={req.MemSid}, PageIndex={req.PageIndex}, PageSize={req.PageSize}");
        }

        private static IElasticOrderSearchService BuildSearchService(IServiceProvider _)
        {
            const string esEndpoint = "http://localhost:9200";
            const string mongoUri = "mongodb://root:example@localhost:27017";
            const string mongoDbName = "CpfOrderDb";

            // ES 端 stack:Driver → Repo → DAL
            var conn = new ConnectionSettings { Elastic = new DbDetail { EndPoint = esEndpoint } };
            var esDriver = new ElasticDriver("Elastic", conn);
            var esRepo = new ElasticRepository<OrderDocument>(esDriver, new ElasticMap(), "orders-*");
            var esDal = new OrderSearchDal(esRepo);

            // Mongo 端 stack (Dual Engine 用、Search 2/3/7 走 Mongo 補資料)
            var mongoClient = new MongoClient(mongoUri);
            var db = mongoClient.GetDatabase(mongoDbName);
            var mongoDal = new MongoSearchDal(
                db.GetCollection<MongoOrder>("Orders"),
                db.GetCollection<MongoUser>("Users"),
                new MongoMap());

            // BLL + Service
            var bll = new ElasticOrderSearchBll(esDal, mongoDal, null, null);
            return new ElasticOrderSearchService(bll, null);
        }

        // ═════════════════════════════════════════════════════════════════
        //  模組 B — 貨態更新服務 (Write)
        //  獨立 ServiceCollection、與模組 A 互不影響。
        //  適用訂單貨態變更 / 物流取號 / 寄貨資訊同步 ($set + $unset + $push 一次合併)
        // ═════════════════════════════════════════════════════════════════
        private static void RunUpdateExample()
        {
            PrintSection("模組 B — 貨態更新服務 (Write)");

            // ───── 模組 B 註冊:組 IMongoDBRepository<OrderModel> ─────
            // 用到的依賴:MongoDBDriver + MongoMap + IDTO + MongoRepository
            // 與模組 A「不重疊」:這裡完全不用 ElasticDriver / IElasticOrderSearchService / MongoSearchDal
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

            using var sp = services.BuildServiceProvider();
            var repo = sp.GetRequiredService<IMongoDBRepository<OrderModel>>();

            // ───── 呼叫範例 ($set + $unset + $push 一次合併) ─────
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

            // ───── dry-run 預覽 (用 SDK public static FlattenBsonDocument 自拼、不執行 I/O) ─────
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

            Console.WriteLine($"  Repository 型別: {repo.GetType().Name}");
            Console.WriteLine($"  filter: {filter}");
            Console.WriteLine("  update 預覽 (合併 $set + $unset + $push):");
            Console.WriteLine(cmd.ToJson(new JsonWriterSettings { Indent = true }));
            Console.WriteLine();
            Console.WriteLine("  生產呼叫: var result = await repo.UpdateData(filter, patch, options);");
        }

        // ─────────────────────────────────────────────────────────────────
        //  輔助 — 視覺分隔
        // ─────────────────────────────────────────────────────────────────
        private static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine(new string('═', 70));
            Console.WriteLine($"  {title}");
            Console.WriteLine(new string('═', 70));
        }

        private static void PrintSection(string title)
        {
            Console.WriteLine();
            Console.WriteLine(new string('─', 70));
            Console.WriteLine($"▶ {title}");
            Console.WriteLine(new string('─', 70));
        }
    }
}
