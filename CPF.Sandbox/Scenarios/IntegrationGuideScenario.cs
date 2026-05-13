using CPF.Service.SendDataToMongoDB.Model.Order;
using Microsoft.Extensions.Configuration;
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
    /// SDK QuickStart 教學 — 對齊 docs/SDK_QuickStart.md 四段結構:
    ///   §0  全局初始化 + 載入 appsettings.json (Global Init + Configuration)
    ///   模組 A — 訂單查詢 SDK (Search)
    ///   模組 B — 貨態更新服務 (Write)
    ///   模組 C — 背景同步服務 (Background Sync) — 詳細實作見 BackgroundServiceScenario.cs
    ///
    /// 連線設定從 CPF.Sandbox/appsettings.json 讀取、不在程式碼中 hardcode。
    /// 模組 A 與模組 B 各自 new ServiceCollection、完全解耦、不共享 DI 容器或任何狀態。
    /// </summary>
    public static class IntegrationGuideScenario
    {
        public static async Task RunAsync()
        {
            PrintHeader("dbSDK QuickStart 教學");

            // §0 全局初始化 + 載入 Configuration
            var settings = RunGlobalInit();

            // 模組 A / B / C 各自吃 settings、不共用 ServiceCollection
            RunSearchExample(settings);
            RunUpdateExample(settings);
            await RunBackgroundExample(settings);

            PrintHeader("教學完成、完整步驟請見 docs/SDK_QuickStart.md");
        }

        // ═════════════════════════════════════════════════════════════════
        //  §0 全局初始化 + 載入 appsettings.json
        //  Program.cs 啟動跑一次、模組 A 與 B 共用 settings。
        // ═════════════════════════════════════════════════════════════════
        private static ConnectionSettings RunGlobalInit()
        {
            PrintSection("§0 全局初始化 + 載入 appsettings.json");

            // 兩階段靜態註冊 (鐵律 §A)
            Console.WriteLine("  Step 1: 兩階段靜態註冊");
            Console.WriteLine("    MongoSerializationConfig.Register();");
            Console.WriteLine("    MongoMap.EnsureClassMapsRegistered();");
            MongoSerializationConfig.Register();
            MongoMap.EnsureClassMapsRegistered();

            // 從 appsettings.json 載入 ConnectionSettings
            Console.WriteLine();
            Console.WriteLine("  Step 2: 從 appsettings.json 載入連線設定");
            Console.WriteLine("    var configuration = new ConfigurationBuilder()");
            Console.WriteLine("        .SetBasePath(AppContext.BaseDirectory)");
            Console.WriteLine("        .AddJsonFile(\"appsettings.json\", optional: false)");
            Console.WriteLine("        .Build();");
            Console.WriteLine("    var settings = new ConnectionSettings();");
            Console.WriteLine("    configuration.GetSection(\"ConnectionSettings\").Bind(settings);");

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var settings = new ConnectionSettings();
            configuration.GetSection("ConnectionSettings").Bind(settings);

            Console.WriteLine();
            Console.WriteLine("  ✅ 載入完成 (顯示讀取結果、密碼省略):");
            Console.WriteLine($"    Mongo.Uri      = {settings.Mongo.Uri}");
            Console.WriteLine($"    Mongo.User     = {settings.Mongo.User}");
            Console.WriteLine($"    Elastic.EndPoint = {settings.Elastic.EndPoint}");

            return settings;
        }

        // ═════════════════════════════════════════════════════════════════
        //  模組 A — 訂單查詢 SDK (Search)
        //  獨立 ServiceCollection、settings 由外層 §0 傳入、不重新讀檔。
        // ═════════════════════════════════════════════════════════════════
        private static void RunSearchExample(ConnectionSettings settings)
        {
            PrintSection("模組 A — 訂單查詢 SDK (Search)");

            // ───── 模組 A 註冊:組 IElasticOrderSearchService ─────
            // 用到的依賴:ElasticDriver + MongoClient + ElasticRepository + MongoSearchDal + Bll + Service
            // 與模組 B「不重疊」:這裡完全不用 MongoDBDriver / IMongoDBRepository
            var services = new ServiceCollection();
            services.AddSingleton(settings);
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

        private static IElasticOrderSearchService BuildSearchService(IServiceProvider sp)
        {
            var settings = sp.GetRequiredService<ConnectionSettings>();

            // ES 端 stack:Driver → Repo → DAL (端點來自 settings.Elastic)
            var esDriver = new ElasticDriver("Elastic", settings);
            var esRepo = new ElasticRepository<OrderDocument>(esDriver, new ElasticMap(), "orders-*");
            var esDal = new OrderSearchDal(esRepo);

            // Mongo 端 stack:Dual Engine 用 (Search 2/3/7 走 Mongo 補資料、連線字串來自 settings.Mongo)
            var mongoConnStr = BuildMongoConnectionString(settings);
            var mongoClient = new MongoClient(mongoConnStr);
            var db = mongoClient.GetDatabase(ExtractDbName(settings.Mongo.Uri));
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
        //  獨立 ServiceCollection、settings 由外層 §0 傳入、不重新讀檔。
        // ═════════════════════════════════════════════════════════════════
        private static void RunUpdateExample(ConnectionSettings settings)
        {
            PrintSection("模組 B — 貨態更新服務 (Write)");

            // ───── 模組 B 註冊:組 IMongoDBRepository<OrderModel> ─────
            // 用到的依賴:MongoDBDriver + MongoMap + IDTO + MongoRepository
            // 與模組 A「不重疊」:這裡完全不用 ElasticDriver / IElasticOrderSearchService / MongoSearchDal
            var services = new ServiceCollection();
            services.AddSingleton(settings);
            services.AddSingleton<MongoDBDriver>(sp =>
                new MongoDBDriver("MongoDB", sp.GetRequiredService<ConnectionSettings>()));
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

        // ═════════════════════════════════════════════════════════════════
        //  模組 C — 背景同步服務 (Background Sync)
        //  Redis Event → Worker → Mongo / ES Repository 全流程模擬。
        //  詳細實作在 BackgroundServiceScenario.cs(避開本檔過長、職責分離)。
        // ═════════════════════════════════════════════════════════════════
        private static async Task RunBackgroundExample(ConnectionSettings settings)
        {
            PrintSection("模組 C — 背景同步服務 (Background Sync)");
            Console.WriteLine("  資料流: Redis Event → Worker → Mongo / ES Repository");
            Console.WriteLine("  本場景用 in-memory Queue 模擬 Redis Buffer、不真連 Redis");
            await BackgroundServiceScenario.RunAsync(settings);
        }

        // ─────────────────────────────────────────────────────────────────
        //  輔助
        // ─────────────────────────────────────────────────────────────────

        // 從 settings.Mongo 組 mongodb:// 連線字串 (走 MongoClient 直接路徑、dev 適用)
        private static string BuildMongoConnectionString(ConnectionSettings s)
            => $"mongodb://{s.Mongo.User}:{s.Mongo.Password}@{s.Mongo.Uri}";

        // 從 Mongo URI 中抽 DB name (例如 "cluster0.xxx/CpfOrderDb" → "CpfOrderDb")
        private static string ExtractDbName(string uri)
        {
            var slashIdx = uri.IndexOf('/');
            if (slashIdx < 0 || slashIdx == uri.Length - 1) return "CpfOrderDb";
            var dbPart = uri.Substring(slashIdx + 1);
            var qIdx = dbPart.IndexOf('?');
            return qIdx >= 0 ? dbPart.Substring(0, qIdx) : dbPart;
        }

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
