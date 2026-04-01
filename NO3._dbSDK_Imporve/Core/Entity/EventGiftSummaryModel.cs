
namespace NO3._dbSDK_Imporve.Core.Entity
{
    /// <summary>
    /// 活動贈品摘要 (精簡版) - 用於列表展示與快速存取
    /// </summary>
    public class EventGiftSummaryModel : OrderSummary
    {
        public string Id { get; set; } // 格式：event_id_gift_id

        // 核心識別
        public string event_id { get; set; }
        public string gift_id { get; set; }

        // 顯示資訊
        public string event_name { get; set; }
        public string item_name { get; set; }
        public string image_path { get; set; } // 活動圖片
        public string gift_image_path { get; set; } // 贈品圖片

        // 關鍵狀態 (N/S/D)
        public string event_status { get; set; }
        public string gift_status { get; set; }

        // 庫存快照
        public int inventory { get; set; } // 總配置數
        public int gift_inventory { get; set; } // 目前剩餘/配置數

        // 時間區間 (用於判斷是否過期)
        public string event_date_s { get; set; }
        public string event_date_e { get; set; }

        // 排序權重
        public int event_sort { get; set; }
        public int sort { get; set; }

        // 系統更新時間
        public string update_date { get; set; }
    }
    public class OrderSummary
    {
    }
}
