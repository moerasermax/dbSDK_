using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;
using System.Text.Json;

namespace CPF.Sandbox.IntegrationTests.PipelineSeeders
{
    /// <summary>
    /// [S37] Golden Data 數據導入：讀取客戶提供的正式測試資料並植入本地環境。
    /// - Elastic：讀取 測試資料_Elastic.txt（Bulk 格式）寫入 orders-605 索引
    /// - Mongo：讀取 測試資料_Mongo.txt（JSON 陣列）寫入 CpfOrderDb.Orders Collection
    /// </summary>
    public static class GoldenSeeder
    {
        private const string ElasticDataPath = ".gemini/Sample_Data/Elastic_Search/測試資料_Elastic.txt";
        private const string MongoDataPath = ".gemini/Sample_Data/Elastic_Search/測試資料_Mongo.txt";
        private const string ElasticIndex = "orders-605";
        private const string MongoDatabase = "CpfOrderDb";
        private const string MongoCollection = "Orders";

        public static async Task SeedAsync(
            string esEndpoint = "http://localhost:9200",
            string mongoUri = "mongodb://root:example@localhost:27017")
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== [S37] Golden Data 數據導入 ===");
            Console.WriteLine("========================================\n");

            // 1. Elastic 植入
            await SeedElasticAsync(esEndpoint);

            // 2. Mongo 植入
            await SeedMongoAsync(mongoUri);

            Console.WriteLine("\n========================================");
            Console.WriteLine("=== Golden Data 導入完成 ===");
            Console.WriteLine("========================================\n");
        }

        #region Elastic 植入

        private static async Task SeedElasticAsync(string esEndpoint)
        {
            Console.WriteLine("📦 [Elastic] 讀取測試資料_Elastic.txt...");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), ElasticDataPath);
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"❌ 檔案不存在：{filePath}");
                return;
            }

            var lines = await File.ReadAllLinesAsync(filePath);
            var documents = ParseElasticBulkFormat(lines);

            Console.WriteLine($"📐 解析完成：{documents.Count} 筆 Document");

            // 連接 ElasticSearch
            var connSettings = new ConnectionSettings
            {
                Elastic = new DbDetail { EndPoint = esEndpoint }
            };
            var driver = new ElasticDriver("Elastic", connSettings);
            var client = driver.GetClient();

            // 建立索引與 Mapping
            await EnsureElasticIndexAsync(client);

            // 批次植入
            int inserted = 0, failed = 0;
            foreach (var doc in documents)
            {
                var resp = await client.IndexAsync(doc, i => i
                    .Index(ElasticIndex)
                    .Id(doc.CoomNo)
                );

                if (resp.IsSuccess())
                    inserted++;
                else
                {
                    failed++;
                    Console.WriteLine($"❌ {doc.CoomNo}: {resp.DebugInformation}");
                }
            }

            // Refresh
            await client.Indices.RefreshAsync(ElasticIndex);

            Console.WriteLine($"✅ Elastic 植入完成：成功 {inserted} 筆，失敗 {failed} 筆");
        }

        /// <summary>
        /// 解析 Elastic Bulk 格式：跳過 index metadata 行，只取 Data 行
        /// 格式：{ "index": { "_id": "xxx" } } → 跳過
        ///       { "coom_no": "xxx", ... } → 解析
        /// </summary>
        private static List<OrderDocument> ParseElasticBulkFormat(string[] lines)
        {
            var documents = new List<OrderDocument>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // 跳過空行與 Bulk API 指令行
                if (string.IsNullOrEmpty(line) ||
                    line.StartsWith("POST") ||
                    line.StartsWith("{ \"index\":"))
                    continue;

                // 嘗試解析為 OrderDocument
                try
                {
                    var doc = JsonSerializer.Deserialize<OrderDocument>(line, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (doc != null && !string.IsNullOrEmpty(doc.CoomNo))
                        documents.Add(doc);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ 第 {i + 1} 行解析失敗：{ex.Message}");
                }
            }

            return documents;
        }

        private static async Task EnsureElasticIndexAsync(ElasticsearchClient client)
        {
            var exists = await client.Indices.ExistsAsync(ElasticIndex);
            if (exists.Exists)
            {
                Console.WriteLine($"🗑️  刪除舊索引：{ElasticIndex}");
                await client.Indices.DeleteAsync(ElasticIndex);
            }

            Console.WriteLine($"📐 建立 Explicit Mapping：{ElasticIndex}");
            var createResp = await client.Indices.CreateAsync(ElasticIndex, c => c
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
                        .LongNumber("coom_rcv_totalamt")
                        .Nested("cood_items", n => n
                            .Properties(np => np
                                .Keyword("cood_name")
                                .LongNumber("cood_qty")
                            )
                        )
                    )
                )
            );

            if (!createResp.IsValidResponse)
            {
                Console.WriteLine($"❌ Mapping 建立失敗：{createResp.DebugInformation}");
                throw new Exception($"無法建立索引 {ElasticIndex}");
            }
        }

        #endregion

        #region Mongo 植入

        private static async Task SeedMongoAsync(string mongoUri)
        {
            Console.WriteLine("\n📦 [Mongo] 讀取測試資料_Mongo.txt...");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), MongoDataPath);
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"❌ 檔案不存在：{filePath}");
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);

            // 解析為 BsonArray
            BsonArray bsonArray;
            try
            {
                bsonArray = BsonSerializer.Deserialize<BsonArray>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ JSON 解析失敗：{ex.Message}");
                return;
            }

            Console.WriteLine($"📐 解析完成：{bsonArray.Count} 筆 Document");

            // 連接 MongoDB
            var client = new MongoClient(mongoUri);
            var database = client.GetDatabase(MongoDatabase);
            var collection = database.GetCollection<BsonDocument>(MongoCollection);

            // 清空舊資料
            Console.WriteLine($"🗑️  清空 Collection：{MongoCollection}");
            await collection.DeleteManyAsync(Builders<BsonDocument>.Filter.Empty);

            // 批次植入
            var documents = bsonArray.Select(b => b.AsBsonDocument).ToList();
            await collection.InsertManyAsync(documents);

            Console.WriteLine($"✅ Mongo 植入完成：{documents.Count} 筆");
        }

        #endregion
    }
}
