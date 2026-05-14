using CPF.Service.SendDataToMongoDB.Model.Order;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
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
    ///   §0  全局初始化 + 載入 appsettings.json + 環境變數覆蓋
    ///   模組 A — 訂單查詢 SDK (Search)
    ///   模組 B — 貨態更新服務 (Write)
    ///
    /// S45 升級:DI 註冊全面採用 services.AddDbSdk(IConfiguration)+ IOptions<ConnectionSettings>;
    /// 環境變數 DBSDK_ConnectionSettings__Mongo__Uri 等可覆蓋 appsettings.json。
    /// 模組 A 與模組 B 各自 new ServiceCollection、完全解耦、不共享 DI 容器或任何狀態。
    /// </summary>
    public static class IntegrationGuideScenario
    {
        public static Task RunAsync()
        {
            PrintHeader("dbSDK QuickStart 教學");

            // §0 全局初始化 + 載入 Configuration
            var (configuration, settings) = RunGlobalInit();

            // 模組 A / B 各自吃 configuration、不共用 ServiceCollection
            var isPlaceholder = IsPlaceholderConfig(settings);
            RunSearchExample(configuration, settings, isPlaceholder);
            RunUpdateExample(configuration, isPlaceholder);

            PrintHeader("教學完成、完整步驟請見 docs/SDK_QuickStart.md");
            return Task.CompletedTask;
        }

        // ═════════════════════════════════════════════════════════════════
        //  §0 全局初始化 + 載入 appsettings.json + 環境變數覆蓋
        //  Program.cs 啟動跑一次、模組 A 與 B 共用 configuration。
        // ═════════════════════════════════════════════════════════════════
        private static (IConfiguration configuration, ConnectionSettings settings) RunGlobalInit()
        {
            PrintSection("§0 全局初始化 + 載入 appsettings.json");

            // 兩階段靜態註冊 (鐵律 §A)
            Console.WriteLine("  Step 1: 兩階段靜態註冊");
            Console.WriteLine("    MongoSerializationConfig.Register();");
            Console.WriteLine("    MongoMap.EnsureClassMapsRegistered();");
            MongoSerializationConfig.Register();
            MongoMap.EnsureClassMapsRegistered();

            // 從 appsettings.json + 環境變數 載入 ConnectionSettings
            Console.WriteLine();
            Console.WriteLine("  Step 2: 載入連線設定 (appsettings.json + 環境變數 prefix DBSDK_ 覆蓋)");
            Console.WriteLine("    var configuration = new ConfigurationBuilder()");
            Console.WriteLine("        .SetBasePath(AppContext.BaseDirectory)");
            Console.WriteLine("        .AddJsonFile(\"appsettings.json\", optional: false)");
            Console.WriteLine("        .AddEnvironmentVariables(prefix: \"DBSDK_\")");
            Console.WriteLine("        .Build();");

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables(prefix: "DBSDK_")
                .Build();

            // 為顯示用而手動 Bind 一次;DI 容器內仍走 IOptions<ConnectionSettings> 模式
            var settings = new ConnectionSettings();
            configuration.GetSection("ConnectionSettings").Bind(settings);

            Console.WriteLine();
            Console.WriteLine("  ✅ 載入完成 (顯示讀取結果、密碼省略):");
            Console.WriteLine($"    Mongo.Uri        = {settings.Mongo.Uri}");
            Console.WriteLine($"    Mongo.User       = {settings.Mongo.User}");
            Console.WriteLine($"    Elastic.EndPoint = {settings.Elastic.EndPoint}");
            Console.WriteLine();
            Console.WriteLine("  💡 環境變數覆蓋範例 (PowerShell):");
            Console.WriteLine("    $env:DBSDK_ConnectionSettings__Mongo__Uri = \"override.example.com/db\"");

            return (configuration, settings);
        }

        // ═════════════════════════════════════════════════════════════════
        //  模組 A — 訂單查詢 SDK (Search)
        //  獨立 ServiceCollection、用 services.AddDbSdk(config)註冊 SDK 基礎元件。
        // ═════════════════════════════════════════════════════════════════
        private static void RunSearchExample(IConfiguration configuration, ConnectionSettings settings, bool isPlaceholder)
        {
            PrintSection("模組 A — 訂單查詢 SDK (Search)");

            // ───── 模組 A 註冊 ─────
            // S45 升級:用 AddDbSdk 統一註冊 Drivers + Maps + IDTO;
            // 額外註冊本模組才需要的 BLL stack(ElasticRepository / Dal / Service)
            Console.WriteLine("  註冊範例:");
            Console.WriteLine("    var services = new ServiceCollection();");
            Console.WriteLine("    services.AddDbSdk(configuration);                                  // SDK 基礎元件");
            Console.WriteLine("    services.AddSingleton<IElasticOrderSearchService>(BuildSearchService); // App-specific");

            if (isPlaceholder)
            {
                Console.WriteLine();
                Console.WriteLine("  ⚠️ appsettings.json 為 placeholder、跳過 ElasticDriver 實例化");
                Console.WriteLine("     替換為真實連線後重跑、即可看到 sdk.SearchByBuyerAsync(req) 完整流程。");
                return;
            }

            var services = new ServiceCollection();
            services.AddDbSdk(configuration);
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

            Console.WriteLine();
            Console.WriteLine($"  服務型別: {sdk.GetType().Name}");
            Console.WriteLine($"  注入呼叫: var result = await sdk.SearchByBuyerAsync(req);");
            Console.WriteLine($"            req.MemSid={req.MemSid}, PageIndex={req.PageIndex}, PageSize={req.PageSize}");
        }

        private static IElasticOrderSearchService BuildSearchService(IServiceProvider sp)
        {
            // 從 DI 容器取 IOptions<ConnectionSettings>;.Value 取出實體
            var settings = sp.GetRequiredService<IOptions<ConnectionSettings>>().Value;
            var esDriver = sp.GetRequiredService<ElasticDriver>();

            // ES 端 stack:Repo → DAL
            var esRepo = new ElasticRepository<OrderDocument>(esDriver, new ElasticMap(), "orders-*");
            var esDal = new OrderSearchDal(esRepo);

            // Mongo 端 stack:Dual Engine 用(Search 2/3/7 走 Mongo 補資料)
            // 用 MongoClient 直連(取 collection 級別 API)、非 SDK 的 IRepository<T> 路徑
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
        //  獨立 ServiceCollection、用 AddDbSdk + AddDbSdkMongoRepository<T> 取代散亂 AddSingleton。
        // ═════════════════════════════════════════════════════════════════
        private static void RunUpdateExample(IConfiguration configuration, bool isPlaceholder)
        {
            PrintSection("模組 B — 貨態更新服務 (Write)");

            // ───── 模組 B 註冊 ─────
            // S45 升級:AddDbSdk 註冊 Drivers + Maps + IDTO;
            // AddDbSdkMongoRepository<OrderModel>("Order")為 entity-specific 註冊(collection 名綁定)
            Console.WriteLine("  註冊範例:");
            Console.WriteLine("    var services = new ServiceCollection();");
            Console.WriteLine("    services.AddDbSdk(configuration);                          // SDK 基礎元件");
            Console.WriteLine("    services.AddDbSdkMongoRepository<OrderModel>(\"Order\");   // entity-specific");
            Console.WriteLine();

            // placeholder 模式:跳過 repo 實例化 (避免 MongoClient 對 placeholder URI 解析失敗)、
            // 仍展示 patch / options / dry-run 完整流程
            IMongoDBRepository<OrderModel>? repo = null;
            if (!isPlaceholder)
            {
                var services = new ServiceCollection();
                services.AddDbSdk(configuration);
                services.AddDbSdkMongoRepository<OrderModel>("Order");

                var sp = services.BuildServiceProvider();
                repo = sp.GetRequiredService<IMongoDBRepository<OrderModel>>();
            }

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

            Console.WriteLine($"  Repository 型別: {(repo?.GetType().Name ?? "(placeholder 模式、未實例化)")}");
            Console.WriteLine($"  filter: {filter}");
            Console.WriteLine("  update 預覽 (合併 $set + $unset + $push):");
            Console.WriteLine(cmd.ToJson(new JsonWriterSettings { Indent = true }));
            Console.WriteLine();
            Console.WriteLine("  生產呼叫: var result = await repo.UpdateData(filter, patch, options);");
        }

        // ─────────────────────────────────────────────────────────────────
        //  輔助
        // ─────────────────────────────────────────────────────────────────

        // 偵測 appsettings.json 是否仍為 placeholder (避免 Driver 對假 URI 炸出)
        private static bool IsPlaceholderConfig(ConnectionSettings s) =>
            string.IsNullOrEmpty(s.Mongo.Uri) ||
            s.Mongo.Uri.Contains("MONGODB_ENDPOINT_URL", StringComparison.OrdinalIgnoreCase) ||
            s.Elastic.EndPoint?.Contains("ELASTIC_ENDPOINT", StringComparison.OrdinalIgnoreCase) == true ||
            s.Mongo.User == "USER_NAME";

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
