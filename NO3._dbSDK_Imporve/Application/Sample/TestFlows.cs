using NO3._dbSDK_Imporve.Application.DTO;
using NO3._dbSDK_Imporve.Application.Sample.Mongo;
using NO3._dbSDK_Imporve.Application.Services;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.External;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using System.Text.Json;

namespace NO3._dbSDK_Imporve.Application.Sample
{
    /// <summary>
    /// 測試流程容器
    /// 將所有測試邏輯從 Program.cs 移出，保持入口點簡潔
    /// </summary>
    public static class TestFlows
    {
        /// <summary>
        /// MongoDB + ElasticSearch CRUD 測試流程
        /// </summary>
        public static async Task RunMongoFlow(
            IRepository<Orders> mongoRepo,
            ElasticRepository<OrderSummary> elasticRepo,
            IDTO dto,
            EventGiftRandomDataGenerator testDataEngine,
            ObjectExtension objectExtension)
        {
            var mongoEngine = new DbSDKEngine<Orders>(mongoRepo);

            Result response;

            EventGiftModel data = testDataEngine.Generate();
            string condition = JsonSerializer.Serialize(dto.GetCondition(data.event_id));

            EventGiftModel devData = objectExtension.Copy(data);
            devData.event_id += "Dev";
            string newDataJson = JsonSerializer.Serialize(devData);

            // MongoDB 測試流程：Insert -> Update -> Read -> Remove
            response = (Result)await mongoEngine.Insert(data);
            Console.WriteLine($"{response.Msg}。請按一下繼續下一步......{data.event_id}"); Console.ReadKey();

            response = (Result)await mongoEngine.Update(condition, devData);
            Console.WriteLine($"{response.Msg}......條件{condition} 更新為{newDataJson}"); Console.ReadKey();

            condition = JsonSerializer.Serialize(dto.GetCondition(devData.event_id));
            response = (Result)await mongoEngine.Read(condition);
            Console.WriteLine($"{response.Msg}。請按一下繼續下一步......{response.DataJson}"); Console.ReadKey();

            response = (Result)await mongoEngine.Remove(condition);
            Console.WriteLine($"已完成資料移除。請按一下結束測試流程......已刪除 {condition}資料"); Console.ReadKey();

            // ElasticSearch 測試流程：Insert -> Update -> Read -> Remove
            var elasticEngine = new DbSDKEngine<OrderSummary>(elasticRepo);
            EventGiftSummaryModel dataSummary = testDataEngine.ToSummary(data);
            string conditionSummary = JsonSerializer.Serialize(dto.GetCondition(dataSummary.event_id));

            EventGiftSummaryModel devDataSummary = objectExtension.Copy(dataSummary);
            devDataSummary.event_id += "Dev";
            string newDataSummaryJson = JsonSerializer.Serialize(devDataSummary);

            response = (Result)await elasticEngine.Insert(dataSummary);
            Console.WriteLine($"{response.Msg}。請按一下繼續下一步......{data.event_id}"); Console.ReadKey();

            response = (Result)await elasticEngine.Update(conditionSummary, devDataSummary);
            Console.WriteLine($"{response.Msg}......條件{conditionSummary} 更新為{newDataSummaryJson}"); Console.ReadKey();

            condition = JsonSerializer.Serialize(dto.GetCondition(devDataSummary.event_id));
            response = (Result)await elasticEngine.Read(condition);
            Console.WriteLine($"{response.Msg}。請按一下繼續下一步......{response.DataJson}"); Console.ReadKey();

            response = (Result)await elasticEngine.Remove(conditionSummary);
            Console.WriteLine($"已完成資料移除。請按一下結束測試流程......已刪除 {condition}資料"); Console.ReadKey();
        }

        /// <summary>
        /// 貨態同步服務測試流程
        /// </summary>
        public static void RunShippingSyncFlow()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== 貨態同步服務測試 ===");
            Console.WriteLine("========================================\n");

            // 測試 1: 管線字串解析
            Console.WriteLine("--- 測試 1: 管線字串解析 ---");
            var dto = new ShippingUpdateDto
            {
                CoomNo = "CM2604160395986",
                CoomStatus = 30,
                EsmmNo = "SM2604160036207",
                EsmmShipNo = "D8803212",
                EsmmStatus = 10,
                EsmlEsmmNoList = "01|2026-04-16T14:11:56.970,10|2026-04-16T14:20:00",
                EsmsEsmmNoList = "1A01|2026-04-16T14:20:00,1001|2026-04-16T14:11:56.970"
            };

            var esmlList = dto.ParseEsmlList();
            var esmsList = dto.ParseEsmsList();

            Console.WriteLine($"貨態歷程筆數: {esmlList.Count}");
            foreach (var item in esmlList)
                Console.WriteLine($"  - 狀態: {item.EsmlEsmmStatus}, 時間: {item.EsmlStatusDatetime:yyyy-MM-dd HH:mm:ss}");

            Console.WriteLine($"物流狀態筆數: {esmsList.Count}");
            foreach (var item in esmsList)
                Console.WriteLine($"  - 狀態碼: {item.EsmsDlvStatusNo}, 時間: {item.EsmsStatusDatetime:yyyy-MM-dd HH:mm:ss}");

            // 測試 2: Status 20 初始化
            Console.WriteLine("\n--- 測試 2: Status 20 初始化 ---");
            var status20Dto = new ShippingUpdateDto
            {
                CoomNo = "CM2604160395986",
                CoomStatus = 20,
                EsmmNo = "SM2604160036207",
                EsmmShipNo = "D8803212",
                EsmmStatus = 1,
                EsmmShipMethod = 1,
                EsmmShipNoAuthCode = 964,
                EsmmShipNoA = "7M0",
                EsmmIbonAppFlag = 0,
                EsmlEsmmNoList = "01|2026-04-16T06:11:56.970",
                EsmsEsmmNoList = "1001|2026-04-16T06:11:56.970"
            };
            Console.WriteLine($"訂單編號: {status20Dto.CoomNo}");
            Console.WriteLine($"訂單狀態: {status20Dto.CoomStatus}");
            Console.WriteLine($"物流單號: {status20Dto.EsmmShipNo}");

            // 測試 3: Status 30 追加
            Console.WriteLine("\n--- 測試 3: Status 30 追加 ---");
            var status30Dto = new ShippingUpdateDto
            {
                CoomNo = "CM2604160395986",
                CoomStatus = 30,
                EsmmStatus = 10,
                EsmlEsmmNoList = "10|2026-04-16T06:20:00",
                EsmsEsmmNoList = "1A01|2026-04-16T06:20:00"
            };
            var status30Esml = status30Dto.ParseEsmlList();
            Console.WriteLine($"追加貨態歷程: {status30Esml.Count} 筆");
            Console.WriteLine($"  - 狀態: {status30Esml[0].EsmlEsmmStatus}, 時間: {status30Esml[0].EsmlStatusDatetime:yyyy-MM-dd HH:mm:ss}");

            Console.WriteLine("\n========================================");
            Console.WriteLine("=== 貨態同步服務測試完成 ===");
            Console.WriteLine("========================================\n");
        }

        /// <summary>
        /// Mock 測試：驗證 $push 指令生成 (不依賴資料庫)
        /// </summary>
        public static void RunMockFlow()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== Mock 測試：貨態歷程追加驗證 ===");
            Console.WriteLine("========================================\n");

            var mockRepo = new MockOrderRepository();

            var status20Data = new MockOrderData
            {
                CoomNo = "CM2604160395986",
                CoomStatus = "20",
                EsmmStatus = "01"
            };

            var status30Options = new MockUpdateOptions
            {
                PushFields = new List<string> { "e_shipment_l", "e_shipment_s" }
            };

            var initResult = mockRepo.UpdateInit(
                @"{""coom_no"": ""CM2604160395986""}",
                status20Data,
                status30Options
            ).Result;

            Console.WriteLine($"測試結果: {initResult.Msg}");
            Console.WriteLine($"指令內容: {initResult.DataJson}");

            var updateDoc = MongoDB.Bson.BsonDocument.Parse(initResult.DataJson);
            var hasPush = updateDoc.Contains("UpdateDefinition") &&
                          updateDoc["UpdateDefinition"].AsBsonDocument.Contains("$push");

            if (hasPush)
            {
                Console.WriteLine("\n✅ Mock 測試通過：$push 指令正確生成");
                Console.WriteLine($"   - $push 內容: {updateDoc["UpdateDefinition"]["$push"]}");
            }
            else
            {
                Console.WriteLine("\n❌ Mock 測試失敗：未找到 $push 指令");
            }

            // 日期格式解析測試
            Console.WriteLine("\n--- 日期格式解析測試 ---");
            var testDates = new[] { "20260416061156.970", "20260416061156", "20260416" };

            foreach (var dateStr in testDates)
            {
                try
                {
                    var parsed = DateTime.ParseExact(dateStr,
                        new[] { "yyyyMMddHHmmss.fff", "yyyyMMddHHmmss", "yyyyMMdd" },
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None);
                    Console.WriteLine($"✅ '{dateStr}' → {parsed:yyyy-MM-dd HH:mm:ss.fff}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ '{dateStr}' 解析失敗: {ex.Message}");
                }
            }

            Console.WriteLine("\n========================================");
            Console.WriteLine("=== Mock 測試完成 ===");
            Console.WriteLine("========================================\n");
        }
    }
}
