using NO3._dbSDK_Imporve.Application.DTO;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// 貨態同步 DTO 解析驗證（不依賴資料庫）
    /// </summary>
    public static class ShippingSyncScenario
    {
        public static void Run()
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
    }
}
