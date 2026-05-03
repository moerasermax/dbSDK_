using CPF.Sandbox.Mocks;
using Elastic.Clients.Elasticsearch.Mapping;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using System.Text.Json;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// CUN9101 樣本資料植入：將 30 筆測試文件寫入 orders-604 index。
    /// 需要本機 ES 在 http://localhost:9200 運行。
    /// </summary>
    public static class ElasticDataSeedScenario
    {
        public static async Task RunAsync(string esEndpoint = "http://localhost:9200")
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== Elastic 資料植入 (CUN9101) ===");
            Console.WriteLine("========================================\n");

            var samplePath = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..",
                    ".gemini", "Sample_Data", "CUN9101", "寫入測試資料_DEVTOOL.txt")
            );

            if (!File.Exists(samplePath))
            {
                Console.WriteLine($"❌ 找不到樣本資料：{samplePath}");
                return;
            }

            var connSettings = new ConnectionSettings
            {
                Elastic = new NO3._dbSDK_Imporve.Core.Models.DbDetail { EndPoint = esEndpoint }
            };
            var driver = new ElasticDriver("Elastic", connSettings);
            var map = new ElasticMap();
            const string indexName = "orders-604";

            await EnsureExplicitMappingAsync(driver, indexName);

            var repo = new ElasticRepository<TestOrderDocument>(driver, map, indexName);

            var lines = await File.ReadAllLinesAsync(samplePath);
            int inserted = 0, failed = 0;

            foreach (var line in lines)
            {
                if (!line.Contains("\"coom_no\"")) continue;

                var doc = JsonSerializer.Deserialize<TestOrderDocument>(line);
                if (doc == null) continue;

                var result = await repo.InsertData(doc);
                if (result.IsSuccess)
                {
                    Console.WriteLine($"✅ {doc.Id}  {result.Msg}");
                    inserted++;
                }
                else
                {
                    Console.WriteLine($"❌ {doc.Id}  {result.Msg}");
                    failed++;
                }
            }

            Console.WriteLine($"\n完成：成功 {inserted} 筆，失敗 {failed} 筆");
            Console.WriteLine("========================================\n");
        }

        /// <summary>
        /// 重建 index 並設定與 production 一致的 explicit mapping。
        /// 字串欄位用 keyword（BLL 用 raw field name 查詢/排序/聚合），
        /// cood_items 用 nested（S29 ReverseNested 聚合需要）。
        /// </summary>
        private static async Task EnsureExplicitMappingAsync(ElasticDriver driver, string indexName)
        {
            var client = driver.GetClient();

            var exists = await client.Indices.ExistsAsync(indexName);
            if (exists.Exists)
            {
                Console.WriteLine($"🗑️  刪除舊 index：{indexName}");
                await client.Indices.DeleteAsync(indexName);
            }

            Console.WriteLine($"📐 建立 explicit mapping：{indexName}");
            var createResp = await client.Indices.CreateAsync(indexName, c => c
                .Mappings(m => m
                    .Dynamic(DynamicMapping.True)
                    .Properties<TestOrderDocument>(p => p
                        .Keyword("coom_no")
                        .Keyword("coom_name")
                        .Keyword("coom_status")
                        .Keyword("coom_temp_type")
                        .Date("coom_create_datetime")
                        .LongNumber("coom_cuam_cid")
                        .Keyword("cooc_no")
                        .Keyword("cooc_payment_type")
                        .Date("cooc_payment_pay_datetime")
                        .Keyword("cooc_deliver_method")
                        .Keyword("cooc_ord_channel_kind")
                        .LongNumber("cooc_mem_sid")
                        .Keyword("cood_name")
                        .Keyword("esmm_ship_no")
                        .Keyword("esmm_status")
                        .Boolean("is_return_shipping")
                        .LongNumber("seller_qa_never_reply_count")
                        .LongNumber("buyer_qa_never_reply_count")
                        .Date("esml_status_shipping_datetime")
                        .Date("esml_status_finish_datetime")
                        .LongNumber("esmm_rcv_total_amt")
                        .Boolean("esms_dlv_status_seller_pickup")
                        .Boolean("esms_sc_status_returning")
                        .Boolean("crsa_applied")
                        .LongNumber("coom_rcv_totalamt")
                        .Date("esmm_leavestoredate_b")
                        .LongNumber("_ord_modify_date")
                        .Nested("cood_items", n => n
                            .Properties(np => np
                                .Keyword("cgdd_cgdmid")
                                .Keyword("cgdd_id")
                                .Keyword("cood_cgdsid")
                                .Keyword("cood_name")
                                .LongNumber("cood_qty")
                                .Keyword("cood_image_path")
                            )
                        )
                    )
                )
            );

            if (!createResp.IsValidResponse)
            {
                Console.WriteLine($"❌ Mapping 建立失敗：{createResp.DebugInformation}");
                throw new Exception("無法建立 explicit mapping");
            }

            Console.WriteLine($"✅ Explicit mapping 建立成功\n");
        }
    }
}
