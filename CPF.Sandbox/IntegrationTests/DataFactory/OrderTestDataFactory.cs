using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace CPF.Sandbox.IntegrationTests.DataFactory
{
    /// <summary>
    /// 100 筆 deterministic 整合測試資料生成器：80 筆背景隨機 + 20 筆 ScenarioPresets。
    /// 同樣 seed → 同樣 100 筆，expected 由 ExpectedValueCalculator 從同一份 dataset 反算。
    /// </summary>
    public static class OrderTestDataFactory
    {
        // 期間：2026-02-01 ~ 2026-04-30 涵蓋 orders-602/603/604 三個 monthly index
        public static readonly DateTime DateRangeStart = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime DateRangeEnd   = new(2026, 4, 30, 23, 59, 59, DateTimeKind.Utc);

        private static readonly string[] StatusPool   = { "10", "10", "10", "10", "30", "30", "11", "12", "1X", "20" };
        private static readonly string[] PaymentPool  = { "1", "1", "1", "4" };
        private static readonly string[] CgdmidPool   = {
            ScenarioPresets.CgdmA, ScenarioPresets.CgdmA, ScenarioPresets.CgdmA, // CgdmA 比重 30%
            ScenarioPresets.CgdmB,
            ScenarioPresets.CgdmC,
            "GM_BG_001", "GM_BG_002", "GM_BG_003", "GM_BG_004", "GM_BG_005"
        };

        // ScenarioPresets 預埋 18 筆，再加 82 筆背景 = 100 筆 dataset
        public static TestDataset Build(int seed = 42, int backgroundCount = 82)
        {
            var rng = new Random(seed);
            var docs = new List<OrderDocument>();

            // 80 筆背景訂單
            for (int i = 0; i < backgroundCount; i++)
            {
                docs.Add(GenerateBackground(rng, i));
            }

            // 20 筆 preset
            docs.AddRange(ScenarioPresets.BuildAll());

            return new TestDataset
            {
                Documents = docs,
                DateRangeStart = DateRangeStart,
                DateRangeEnd = DateRangeEnd,
                SellerCuamCid = ScenarioPresets.SellerCuamCid,
                BuyerMemSid   = ScenarioPresets.BuyerMemSid,
                Seed = seed
            };
        }

        private static OrderDocument GenerateBackground(Random rng, int idx)
        {
            // 日期分布：random 在 2/1 ~ 4/30
            var totalDays = (int)(DateRangeEnd - DateRangeStart).TotalDays;
            var dayOffset = rng.Next(0, totalDays + 1);
            var hour = rng.Next(0, 24);
            var minute = rng.Next(0, 60);
            var createUtc = DateRangeStart.AddDays(dayOffset).AddHours(hour).AddMinutes(minute);

            var status      = StatusPool[rng.Next(StatusPool.Length)];
            var paymentType = PaymentPool[rng.Next(PaymentPool.Length)];
            var cgdmid      = CgdmidPool[rng.Next(CgdmidPool.Length)];
            var amount      = rng.Next(50, 5000);
            var qty         = rng.Next(1, 5);

            // 60% 賣家 = 528672；其他賣家
            var cuamCid = rng.NextDouble() < 0.6 ? ScenarioPresets.SellerCuamCid : 528600 + rng.Next(0, 100);
            var memSid  = rng.NextDouble() < 0.6 ? ScenarioPresets.BuyerMemSid   : 528600 + rng.Next(0, 100);

            // 對齊客戶 CUN9101 範例：esmm_* 欄位只在 status=30 或 (status=20 + 隨機) 才有
            var hasShip = status == "30" || (status == "20" && rng.NextDouble() < 0.5);
            var esmmStatus = hasShip ? new[] { "10", "20", "30", "11" }[rng.Next(4)] : null;
            // 對齊客戶 CUN9101 範例：pay_datetime 只在 status=11/30 + payment_type=4 才有（其他不填）
            var hasPayDatetime = (status == "11" || status == "30") && paymentType == "4";

            // coom_no 格式對齊 production：CM + yy(2) + mm(2) + 9-digit 流水 = 15 字符
            // 第 4-6 字符 (yy 末碼+mm) 是 OrderIndexRouter.FromCoomNo 路由用
            // background idx + 1000 避開 preset 用的 1~99 範圍
            var coomNo = $"CM{createUtc.Year % 100:D2}{createUtc.Month:D2}{(idx + 1000):D9}";

            return new OrderDocument
            {
                CoomNo = coomNo,
                CoomName = $"BG-{idx}",
                CoomStatus = status,
                CoomTempType = "01",
                CoomCreateDatetime = createUtc,
                CoomCuamCid = cuamCid,
                CoocNo = $"CC_{coomNo}",
                CoocPaymentType = paymentType,
                CoocPaymentPayDatetime = hasPayDatetime ? createUtc.AddSeconds(rng.Next(1, 600)) : null,
                CoocDeliverMethod = "1",
                CoocOrdChannelKind = (rng.Next(1, 7)).ToString(),
                CoocMemSid = memSid,
                CoodNames = new[] { $"bg-item-{idx}" },
                EsmmShipNo = hasShip ? $"SHIP_BG_{idx:D3}" : null,
                EsmmStatus = esmmStatus,
                EsmlStatusShippingDateTime = hasShip ? createUtc.AddHours(2) : null,
                // 對齊客戶 CUN9101 範例：esmm_rcv_total_amt 只在已出貨時填
                EsmmRcvTotalAmt = hasShip ? amount : null,
                CoomRcvTotalAmt = amount,
                // 客戶範例：is_return_shipping 只在 true 時才出現於 doc，false 不寫
                IsReturnShipping = esmmStatus == "31" ? true : (bool?)null,
                CoodItems = new[]
                {
                    new CoodItems
                    {
                        CgddCgdmid = cgdmid,
                        // cgdd_id 與 cgdmid 綁定（同商品共用），讓 ProductRanking terms agg 有意義
                        CgddId = $"cgdd_{cgdmid}",
                        CoodCgdsId = $"cgds_{cgdmid}",
                        CoodName = $"bg-item-{cgdmid}",
                        CoodQty = qty,
                        CoodImagePath = $"bg_{cgdmid}.jpg"
                    }
                }
            };
        }
    }
}
