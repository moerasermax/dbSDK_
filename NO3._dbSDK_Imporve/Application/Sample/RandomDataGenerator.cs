using NO3._dbSDK_Imporve.Core.Entity;


namespace NO3._dbSDK_Imporve.Application.Sample
{

    public class RandomDataGenerator
    {


        public static class EventGiftGenerator
        {
            private static readonly Random _random = new Random();
            public static EventGiftModel Generate()
            {
                return CreateRandomItem_Mongo();
            }
            /// <summary>
            /// 產生指定數量的隨機活動贈品資料
            /// </summary>
            public static List<EventGiftModel> Generate_Mongo(int count)
            {
                var list = new List<EventGiftModel>();

                for (int i = 0; i < count; i++)
                {
                    var item = CreateRandomItem_Mongo();
                    list.Add(item);
                }

                return list;
            }
            private static EventGiftModel CreateRandomItem_Mongo()
            {
                // 基礎識別碼
                string eventId = $"EVT{_random.Next(1000, 9999)}";
                string giftId = $"GFT{_random.Next(10000, 99999)}";

                // 模擬日期 (yyyyMMddHHmmss.fff)
                string nowStr = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
                string startDate = DateTime.Now.AddDays(_random.Next(-10, 0)).ToString("yyyyMMddHHmmss.fff");
                string endDate = DateTime.Now.AddDays(_random.Next(1, 30)).ToString("yyyyMMddHHmmss.fff");

                return new EventGiftModel
                {
                    // 1. 手動實作 _id (串接 PK_SK)
                    Id = $"{eventId}_{giftId}",

                    // 2. 基礎資訊
                    event_id = eventId,
                    gift_id = giftId,
                    event_name = $"隨機活動_{Guid.NewGuid().ToString().Substring(0, 5)}",
                    event_intro = "這是活動簡介內容",
                    event_desc = "這是活動詳細說明範例",
                    event_notice = "注意事項：請於期限內兌換",
                    event_remark = "備註資訊",
                    event_target = "全體會員",
                    image_path = $"/images/event/{eventId}.jpg",

                    // 3. 狀態與類型
                    show_type = _random.Next(1, 3),  // 1 或 2
                    run_type = _random.Next(1, 3),   // 1 或 2
                    event_sort = _random.Next(1, 100),
                    event_status = GetRandomFrom("N", "S", "D"),
                    process_status = "READY",
                    is_release = GetRandomFrom("Y", "N"),
                    is_archive = "N",

                    // 4. 庫存與數量
                    user_gift_limit = _random.Next(1, 5),
                    user_gift_limit_daily = 1,
                    inventory = _random.Next(100, 1000),
                    gift_inventory = _random.Next(10, 100),
                    daily_restriction_amount = _random.Next(5, 20),

                    // 5. 贈品詳細
                    item_name = $"贈品_{_random.Next(100, 999)}號",
                    item_code = $"CODE_{Guid.NewGuid().ToString().ToUpper().Substring(0, 8)}",
                    gift_sort = _random.Next(1, 50),
                    gift_status = "Y",
                    gift_image_path = "/images/gift/sample.png",
                    card_remark = "卡面描述文字",

                    // 6. 外部與 Pool
                    source_business = "MARKETING",
                    pool_id = $"POOL_{_random.Next(1, 99)}",
                    pool_name = "主要贈品池",
                    egtc_condition_1 = "NONE",

                    // 7. 通路與條碼
                    exchange_channel = "APP",
                    exchange_channel_url = "https://example.com/redeem",
                    order_barcode_type = 1,
                    order_barcode = "1234567890123",
                    order_item_no = "ITEM_NO_999",

                    // 8. 其他屬性
                    no_type = 1,
                    imm_type = "1",
                    add_amount = 0,
                    inventory_type = "REALTIME",
                    store_service_id = "STORE_001",
                    is_show_coupon = "Y",
                    sort = _random.Next(1, 100),

                    // 9. 日期區
                    event_date_s = startDate,
                    event_date_e = endDate,
                    gift_date_s = startDate,
                    gift_date_e = endDate,
                    trade_date_s = startDate,
                    trade_date_e = endDate,
                    gift_archive_date = "",
                    update_date = nowStr,
                    create_date = nowStr
                };
            }
            public static EventGiftSummaryModel ToSummary(EventGiftModel full)
            {

                return new EventGiftSummaryModel
                {
                    Id = full.Id,
                    event_id = full.event_id,
                    gift_id = full.gift_id,
                    event_name = full.event_name,
                    item_name = full.item_name,
                    image_path = full.image_path,
                    gift_image_path = full.gift_image_path,
                    event_status = full.event_status,
                    gift_status = full.gift_status,
                    inventory = full.inventory,
                    gift_inventory = full.gift_inventory,
                    event_date_s = full.event_date_s,
                    event_date_e = full.event_date_e,
                    event_sort = full.event_sort,
                    sort = full.sort,
                    update_date = full.update_date
                };

            }
            private static string GetRandomFrom(params string[] options)
            {
                return options[_random.Next(options.Length)];
            }
        }


        public static RandomDataGenerator getInstance() { return _instance; }
        private static RandomDataGenerator _instance = new RandomDataGenerator();
        private RandomDataGenerator() { }
    }
}
