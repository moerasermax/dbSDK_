using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace CPF.Sandbox.IntegrationTests.DataFactory
{
    /// <summary>
    /// 預埋 20 筆「特殊場景」doc，每筆用途明確，對齊 GoldenRecipe 語意。
    /// coom_no 格式必須符合 OrderIndexRouter.FromCoomNo 約束：
    ///   "CM" + yy + mm + 9-digit 流水（第 4~6 字符 = 年末碼 + 月份兩碼）
    /// </summary>
    public static class ScenarioPresets
    {
        public const int SellerCuamCid = 528672;
        public const int BuyerMemSid = 528672;

        public const string CgdmA = "GM2508260014245";  // 主要商品 (S7 Cgdm[0])
        public const string CgdmB = "GM2512180014259";  // 次要商品 (S7 Cgdm[1])
        public const string CgdmC = "GM2512170027503";  // S5/S6 商品

        public static IReadOnlyList<OrderDocument> BuildAll()
        {
            var list = new List<OrderDocument>();

            // ===== Search 5 (Today) 預埋：04/29 Taiwan 16:30 = UTC 08:30 amt=88 =====
            list.Add(MakePreset(
                coomNo: "CM2604000000001",
                createUtc: new DateTime(2026, 4, 29, 8, 30, 0, DateTimeKind.Utc),
                status: "10", paymentType: "1",
                amount: 88, cgdmid: CgdmC, qty: 5
            ));

            // ===== Search 6 (Week) 預埋：04/28 Taiwan 16:30 = UTC 08:30 amt=88 =====
            list.Add(MakePreset(
                coomNo: "CM2604000000002",
                createUtc: new DateTime(2026, 4, 28, 8, 30, 0, DateTimeKind.Utc),
                status: "10", paymentType: "1",
                amount: 88, cgdmid: CgdmC, qty: 5
            ));

            // ===== Search 7 預埋：CgdmA 與 CgdmB 提供 cood_items cgdmid 多樣性 =====
            // (不預埋 _ord_modify_date — 客戶 sample 沒給,我們也不自加)
            list.Add(MakePreset(
                coomNo: "CM2604000000003",
                createUtc: new DateTime(2026, 4, 23, 3, 11, 0, DateTimeKind.Utc),
                status: "10", paymentType: "1",
                amount: 138, cgdmid: CgdmA, qty: 1
            ));
            list.Add(MakePreset(
                coomNo: "CM2604000000004",
                createUtc: new DateTime(2026, 4, 23, 3, 29, 0, DateTimeKind.Utc),
                status: "10", paymentType: "1",
                amount: 138, cgdmid: CgdmB, qty: 1
            ));

            // ===== Search 4 (Dashboard) 預埋：涵蓋 NewOrder/Shipped/Replied 各態樣 =====
            list.Add(MakePreset(
                coomNo: "CM2604000000005",
                createUtc: new DateTime(2026, 4, 20, 5, 0, 0, DateTimeKind.Utc),
                status: "10", paymentType: "4",  // 未付款（NewOrder）
                amount: 200, cgdmid: CgdmA, qty: 1
            ));
            list.Add(MakePreset(
                coomNo: "CM2604000000006",
                createUtc: new DateTime(2026, 4, 21, 5, 0, 0, DateTimeKind.Utc),
                status: "30", paymentType: "1",  // 已寄出（Shipped）
                amount: 250, cgdmid: CgdmA, qty: 2,
                esmmShipNo: "SHIP_PRE_01", esmmStatus: "10"
            ));
            list.Add(MakePreset(
                coomNo: "CM2604000000007",
                createUtc: new DateTime(2026, 4, 22, 5, 0, 0, DateTimeKind.Utc),
                status: "10", paymentType: "1",
                amount: 100, cgdmid: CgdmA, qty: 1,
                sellerQaNeverReplyCount: 2  // QA 待回覆
            ));

            // ===== Search 1 (HomeOverview) 預埋：BuyerPickup（取貨待領）=====
            list.Add(MakePreset(
                coomNo: "CM2604000000008",
                createUtc: new DateTime(2026, 4, 24, 5, 0, 0, DateTimeKind.Utc),
                status: "30", paymentType: "1",
                amount: 150, cgdmid: CgdmA, qty: 1,
                esmmShipNo: "PICKUP_01", esmmStatus: "20"
            ));

            // ===== Search 2/3 sort 第一筆 deterministic：create 最新 + coom_no 最大 =====
            list.Add(MakePreset(
                coomNo: "CM2604999999999",
                createUtc: new DateTime(2026, 4, 30, 23, 59, 59, DateTimeKind.Utc),
                status: "10", paymentType: "1",
                amount: 999, cgdmid: CgdmA, qty: 1
            ));

            // ===== 額外 9 筆 CgdmA 補強（涵蓋 3 月，測 OrderIndexRouter 跨月）=====
            for (int i = 0; i < 9; i++)
            {
                var dt = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc).AddDays(i);
                list.Add(MakePreset(
                    coomNo: $"CM2603{(100 + i):D9}",
                    createUtc: dt,
                    status: "10", paymentType: "1",
                    amount: 100 + i * 10,
                    cgdmid: CgdmA, qty: 1
                ));
            }

            return list;  // 共 20 筆
        }

        private static OrderDocument MakePreset(
            string coomNo,
            DateTime createUtc,
            string status,
            string paymentType,
            int amount,
            string cgdmid,
            int qty,
            long? ordModifyDate = null,
            string? esmmShipNo = null,
            string? esmmStatus = null,
            int? sellerQaNeverReplyCount = null,
            DateTime? coocPaymentPayDatetime = null)
        {
            // 對齊客戶 CUN9101 範例 format：
            // - cooc_payment_pay_datetime 只在客戶範例 status=11 + payment=4 時才有，預設不填
            // - esmm_rcv_total_amt 只在有 esmm_ship_no (已出貨) 時才有
            return new OrderDocument
            {
                CoomNo = coomNo,
                CoomName = "整合測試-預埋",
                CoomStatus = status,
                CoomTempType = "01",
                CoomCreateDatetime = createUtc,
                CoomCuamCid = SellerCuamCid,
                CoocNo = $"CC_{coomNo}",
                CoocPaymentType = paymentType,
                CoocPaymentPayDatetime = coocPaymentPayDatetime,
                CoocDeliverMethod = "1",
                CoocOrdChannelKind = "1",
                CoocMemSid = BuyerMemSid,
                CoodNames = new[] { $"item-{cgdmid}" },
                EsmmShipNo = esmmShipNo,
                EsmmStatus = esmmStatus,
                EsmlStatusShippingDateTime = esmmShipNo != null ? createUtc.AddHours(2) : null,
                EsmmRcvTotalAmt = esmmShipNo != null ? amount : null,
                CoomRcvTotalAmt = amount,
                SellerQaNeverReplyCount = sellerQaNeverReplyCount,
                OrdModifyDate = ordModifyDate,
                CoodItems = new[]
                {
                    new CoodItems
                    {
                        CgddCgdmid = cgdmid,
                        // cgdd_id 與 cgdmid 綁定（同一商品共用 cgdd_id），讓 ProductRanking terms agg 有意義
                        CgddId = $"cgdd_{cgdmid}",
                        CoodCgdsId = $"cgds_{cgdmid}",
                        CoodName = $"item-{cgdmid}",
                        CoodQty = qty,
                        CoodImagePath = $"img_{cgdmid}.jpg"
                    }
                }
            };
        }
    }
}
