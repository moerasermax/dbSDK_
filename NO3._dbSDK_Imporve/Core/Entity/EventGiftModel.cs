
namespace NO3._dbSDK_Imporve.Core.Entity
{
    public class EventGiftModel : Orders
    {
        // 仍建議保留此標記，明確指定此屬性對應 MongoDB 的 _id 欄位

        public string id { get; set; }

        // 1. 基礎資訊
        public string event_id { get; set; }
        public string gift_id { get; set; }
        public string event_name { get; set; }
        public string event_intro { get; set; }
        public string event_desc { get; set; }
        public string event_notice { get; set; }
        public string event_remark { get; set; }
        public string event_target { get; set; }
        public string image_path { get; set; }

        // 2. 狀態與類型 (數值型)
        public int show_type { get; set; }
        public int run_type { get; set; }
        public int event_sort { get; set; }
        public string event_status { get; set; }
        public string process_status { get; set; }
        public string is_release { get; set; }
        public string is_archive { get; set; }

        // 3. 庫存與數量
        public int user_gift_limit { get; set; }
        public int user_gift_limit_daily { get; set; }
        public int inventory { get; set; }
        public int gift_inventory { get; set; }
        public int daily_restriction_amount { get; set; }

        // 4. 贈品詳細
        public string item_name { get; set; }
        public string item_code { get; set; }
        public int gift_sort { get; set; }
        public string gift_status { get; set; }
        public string gift_image_path { get; set; }
        public string card_remark { get; set; }

        // 5. 外部與 Pool
        public string source_business { get; set; }
        public string pool_id { get; set; }
        public string pool_name { get; set; }
        public string egtc_condition_1 { get; set; }

        // 6. 通路與條碼
        public string exchange_channel { get; set; }
        public string exchange_channel_url { get; set; }
        public int order_barcode_type { get; set; }
        public string order_barcode { get; set; }
        public string order_item_no { get; set; }

        // 7. 其他屬性
        public int no_type { get; set; }
        public string imm_type { get; set; }
        public int add_amount { get; set; }
        public string inventory_type { get; set; }
        public string store_service_id { get; set; }
        public string is_show_coupon { get; set; }
        public int sort { get; set; }

        // 8. 日期區 (String 格式)
        public string event_date_s { get; set; }
        public string event_date_e { get; set; }
        public string gift_date_s { get; set; }
        public string gift_date_e { get; set; }
        public string trade_date_s { get; set; }
        public string trade_date_e { get; set; }
        public string gift_archive_date { get; set; }
        public string update_date { get; set; }
        public string create_date { get; set; }
    }

    public class Orders
    {
        /// <summary>
        /// 主鍵 (訂單編號)
        /// </summary>
        public string? PK { get; set; }

        /// <summary>
        /// 訂單主檔
        /// </summary>
        public C_Order_M_Model? C_Order_M { get; set; }

        /// <summary>
        /// 訂單聯絡資訊
        /// </summary>
        public C_Order_C_Model? C_Order_C { get; set; }

        /// <summary>
        /// 訂單明細清單
        /// </summary>
        public List<C_Order_D_Model>? C_Order_D { get; set; }

        /// <summary>
        /// 商品項目
        /// </summary>
        public C_Goods_Item_Model? C_Goods_Item { get; set; }

        /// <summary>
        /// 物流主檔
        /// </summary>
        public E_Shipment_M_Model? E_Shipment_M { get; set; }

        /// <summary>
        /// 貨態歷程清單
        /// </summary>
        public List<E_Shipment_L_Model>? E_Shipment_L { get; set; }

        /// <summary>
        /// 物流狀態清單
        /// </summary>
        public List<E_Shipment_S_Model>? E_Shipment_S { get; set; }
    }
}
