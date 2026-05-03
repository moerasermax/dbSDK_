using CPF.Sandbox.IntegrationTests.DataFactory;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace CPF.Sandbox.IntegrationTests.PipelineSeeders
{
    /// <summary>
    /// 整合測試 ES 植入：依 doc.CoomCreateDatetime 路由到對應的 monthly index
    /// (orders-602/603/604)，並建立與 production 一致的 explicit mapping。
    /// </summary>
    public static class ElasticSeeder
    {
        public static async Task SeedAsync(TestDataset dataset, string esEndpoint = "http://localhost:9200")
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== 整合測試 ES 植入 (100 筆) ===");
            Console.WriteLine("========================================\n");

            var connSettings = new ConnectionSettings
            {
                Elastic = new DbDetail { EndPoint = esEndpoint }
            };
            var driver = new ElasticDriver("Elastic", connSettings);
            var client = driver.GetClient();

            // 1. 算出涉及的 monthly indices
            var indicesNeeded = dataset.Documents
                .Select(d => MonthIndexFor(d.CoomCreateDatetime!.Value))
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            Console.WriteLine($"📐 需建立 {indicesNeeded.Count} 個 monthly index：{string.Join(", ", indicesNeeded)}\n");

            // 2. 對每個 index：DELETE + CREATE explicit mapping
            foreach (var indexName in indicesNeeded)
            {
                await EnsureExplicitMappingAsync(client, indexName);
            }

            // 3. 對每個 doc：路由到對應 index 並插入
            int inserted = 0, failed = 0;
            foreach (var doc in dataset.Documents)
            {
                var indexName = MonthIndexFor(doc.CoomCreateDatetime!.Value);
                var resp = await client.IndexAsync(doc, i => i
                    .Index(indexName)
                    .Id(doc.CoomNo)
                );

                if (resp.IsSuccess())
                {
                    inserted++;
                }
                else
                {
                    failed++;
                    Console.WriteLine($"❌ {doc.CoomNo} → {indexName}: {resp.DebugInformation}");
                }
            }

            // 4. refresh 全部 index 確保查得到
            foreach (var indexName in indicesNeeded)
            {
                await client.Indices.RefreshAsync(indexName);
            }

            Console.WriteLine($"\n完成：成功 {inserted} 筆，失敗 {failed} 筆（共 {dataset.Documents.Count} 筆 dataset）");
            Console.WriteLine("========================================\n");
        }

        /// <summary>依 OrderIndexRouter 約定：orders-{year%10}{month:D2}</summary>
        private static string MonthIndexFor(DateTime dt) => $"orders-{dt.Year % 10}{dt.Month:D2}";

        private static async Task EnsureExplicitMappingAsync(ElasticsearchClient client, string indexName)
        {
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
                    .Properties<OrderDocument>(p => p
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
                throw new Exception($"無法建立 explicit mapping for {indexName}");
            }
        }
    }
}
