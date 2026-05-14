using CPF.Service.SendDataToMongoDB.Model.Order;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Interfaces;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Models;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serialization;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// S42 進階更新範例:示範 $set + $unset 一次性執行的複合操作。
    ///
    /// 業務情境:訂單狀態從「待支付」變為「已支付」
    ///   - $set:c_order_m.coom_status (狀態)+ c_order_m.coom_seller_memo (付款備註)
    ///   - $unset:c_order_m.coom_payment_due_reminder (移除已過期的逾期提醒欄位)
    ///
    /// 紀律:
    ///   - 採取 S45 新 DI 模式 services.AddDbSdk(config)+ AddDbSdkMongoRepository&lt;T&gt;("Order")
    ///   - 對齊 DBSDK Part I §D:複合操作須一次執行、嚴禁拆分為多次 SDK 呼叫
    ///   - 對齊 DBSDK Part I §C:Flatten 自動忽略 null 欄位 (避免覆蓋舊值)
    ///   - 「coom_payment_due_reminder」為示意 ghost field:OrderModel 未顯式定義、
    ///     展示 NoSQL schema-on-read 對舊欄位的清理路徑。
    ///
    /// 執行模式:
    ///   - 若 appsettings.json 仍是 placeholder (MONGODB_ENDPOINT_URL 等),只跑 dry-run 預覽
    ///   - 若已配置真實 Mongo 連線,額外執行實際 UpdateData 並印 Result
    /// </summary>
    public static class AdvancedUpdateScenario
    {
        private const string DemoCoomNo = "CM2604160395986";
        private const string PaidStatusCode = "20";
        private const string GhostReminderField = "c_order_m.coom_payment_due_reminder";

        public static async Task RunAsync()
        {
            PrintHeader("S42 進階更新範例:$set + $unset 一次性複合操作");

            // §0 兩階段靜態初始化 + 載入 configuration
            MongoSerializationConfig.Register();
            MongoMap.EnsureClassMapsRegistered();

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables(prefix: "DBSDK_")
                .Build();

            // §1 S45 模式註冊:AddDbSdk 統一裝載 + AddDbSdkMongoRepository<OrderModel>
            var services = new ServiceCollection();
            services.AddDbSdk(configuration);
            services.AddDbSdkMongoRepository<OrderModel>("Order");

            // §2 組 patch + options (示範 $set + $unset)
            var (patch, options, filter) = BuildPatchAndOptions();

            // §3 dry-run 預覽 (用 SDK public static FlattenBsonDocument 自拼)
            var cmd = ComposeUpdateCommand(patch, options);
            PrintDryRun(filter, cmd);

            // §4 placeholder 偵測:只讀 Mongo.Uri 字串、避開 Redis.Port int 嚴格綁定
            var mongoUri = configuration["ConnectionSettings:Mongo:Uri"];
            var mongoUser = configuration["ConnectionSettings:Mongo:User"];
            if (IsPlaceholderConfig(mongoUri, mongoUser))
            {
                PrintSection("§4 結論:dry-run 完成 (appsettings.json 為 placeholder、未連 Mongo)");
                Console.WriteLine("  將 appsettings.json 替換為真實連線後重跑、可觀察實際 $set / $unset 效果。");
                return;
            }

            // §5 真實執行 (僅當 placeholder 已替換)
            await using var sp = services.BuildServiceProvider();
            var repo = sp.GetRequiredService<IMongoDBRepository<OrderModel>>();

            PrintSection("§5 實際執行 UpdateData (filter, patch, options)");
            var result = await repo.UpdateData(filter, patch, options);
            Console.WriteLine($"  IsSuccess : {result.IsSuccess}");
            Console.WriteLine($"  Msg       : {result.Msg}");
            Console.WriteLine($"  DataJson  : {result.DataJson}");
            Console.WriteLine();
            Console.WriteLine("  💡 驗證方式:用 Studio 3T / mongo shell 查 coom_no=" + DemoCoomNo);
            Console.WriteLine($"     - 預期 c_order_m.coom_status = \"{PaidStatusCode}\"");
            Console.WriteLine($"     - 預期 c_order_m.coom_seller_memo 含付款備註");
            Console.WriteLine($"     - 預期 {GhostReminderField} 欄位不存在 (已被 $unset 移除)");
        }

        // ─────────────────────────────────────────────────────────────────
        //  Patch + Options 建構 (示範 $set + $unset 同時使用)
        // ─────────────────────────────────────────────────────────────────
        private static (OrderModel patch, MongoUpdateOptions options, string filter) BuildPatchAndOptions()
        {
            string filter = $"{{ \"coom_no\": \"{DemoCoomNo}\" }}";

            // $set 部分:用強型別 OrderModel patch、Flatten 自動扁平化 + 忽略 null
            var patch = new OrderModel
            {
                PK = DemoCoomNo,
                C_Order_M = new C_Order_M_Model
                {
                    CoomStatus = PaidStatusCode,
                    CoomSellerMemo = $"付款已確認 ({DateTime.UtcNow:yyyy-MM-dd HH:mm}Z)"
                    // 其他 nullable 欄位留 null、Flatten 會跳過、不會覆蓋舊值
                }
            };

            // $unset 部分:用 UnsetFields 列「點符號路徑」清單
            var options = new MongoUpdateOptions
            {
                IsUpsert = false,             // 純更新、不存在不建
                UnsetFields = new List<string>
                {
                    GhostReminderField        // 移除已過期的逾期提醒欄位
                }
            };

            return (patch, options, filter);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Composer:預覽 $set + $unset 合併指令 (對齊 SDK 內部行為)
        // ─────────────────────────────────────────────────────────────────
        private static BsonDocument ComposeUpdateCommand(OrderModel patch, MongoUpdateOptions options)
        {
            var rawPatch = patch.ToBsonDocument();
            var flatSet = MongoRepository<OrderModel>.FlattenBsonDocument(rawPatch);

            var cmd = new BsonDocument { ["$set"] = flatSet };

            if (options.UnsetFields?.Count > 0)
            {
                var unset = new BsonDocument();
                foreach (var f in options.UnsetFields) unset.Add(f, "");
                cmd["$unset"] = unset;
            }

            return cmd;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Dry-run 印 + 紀律驗證 (FlattenBsonDocument 忽略 null 規範)
        // ─────────────────────────────────────────────────────────────────
        private static void PrintDryRun(string filter, BsonDocument cmd)
        {
            PrintSection("§3 dry-run 預覽 (合併 $set + $unset)");
            Console.WriteLine($"  filter : {filter}");
            Console.WriteLine();
            Console.WriteLine("  update cmd:");
            Console.WriteLine(cmd.ToJson(new JsonWriterSettings { Indent = true }));
            Console.WriteLine();

            // VCP 紀律驗證:Flatten 必忽略 null + $set $unset 同時存在
            var setDoc = cmd["$set"].AsBsonDocument;
            bool hasSet = cmd.Contains("$set") && setDoc.ElementCount > 0;
            bool hasUnset = cmd.Contains("$unset") && cmd["$unset"].AsBsonDocument.ElementCount > 0;
            bool noNullInSet = !setDoc.Any(e => e.Value.IsBsonNull);
            bool hasStatusPath = setDoc.Any(e => e.Name == "c_order_m.coom_status");
            bool hasMemoPath = setDoc.Any(e => e.Name == "c_order_m.coom_seller_memo");
            bool hasGhostUnset = hasUnset && cmd["$unset"].AsBsonDocument.Contains(GhostReminderField);

            Console.WriteLine("  紀律驗證:");
            Console.WriteLine($"    {Pass(hasSet)} $set 段存在且有欄位");
            Console.WriteLine($"    {Pass(hasUnset)} $unset 段存在且有欄位");
            Console.WriteLine($"    {Pass(noNullInSet)} $set 內無 null 值 (Flatten 規範 — DBSDK Part I §C)");
            Console.WriteLine($"    {Pass(hasStatusPath)} $set 含 c_order_m.coom_status");
            Console.WriteLine($"    {Pass(hasMemoPath)} $set 含 c_order_m.coom_seller_memo");
            Console.WriteLine($"    {Pass(hasGhostUnset)} $unset 含 {GhostReminderField}");
        }

        // ─────────────────────────────────────────────────────────────────
        //  Placeholder 偵測 (避免在 dev 環境跑連不上的 UpdateData)
        // ─────────────────────────────────────────────────────────────────
        private static bool IsPlaceholderConfig(string? mongoUri, string? mongoUser) =>
            string.IsNullOrEmpty(mongoUri) ||
            mongoUri.Contains("MONGODB_ENDPOINT_URL", StringComparison.OrdinalIgnoreCase) ||
            mongoUser == "USER_NAME";

        // ─────────────────────────────────────────────────────────────────
        //  輔助
        // ─────────────────────────────────────────────────────────────────
        private static string Pass(bool ok) => ok ? "✅ PASS" : "❌ FAIL";

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
