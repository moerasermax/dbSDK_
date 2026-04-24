using System.Globalization;
using System.Text.Json.Serialization;

namespace NO3._dbSDK_Imporve.Application.DTO
{
    /// <summary>
    /// 貨態更新 DTO (從 Redis 管線字串解析)
    /// 支援 Status 20 (賣家取號) 與 Status 30 (寄貨完成)
    /// </summary>
    public class ShippingUpdateDto
    {
        /// <summary>
        /// 訂單編號
        /// </summary>
        [JsonPropertyName("Coom_No")]
        public string? CoomNo { get; set; }

        /// <summary>
        /// 訂單狀態 (20: 取號, 30: 寄貨)
        /// </summary>
        [JsonPropertyName("Coom_Status")]
        public int? CoomStatus { get; set; }

        /// <summary>
        /// 物流主檔編號
        /// </summary>
        [JsonPropertyName("Esmm_No")]
        public string? EsmmNo { get; set; }

        /// <summary>
        /// 物流狀態 (01: 已取號, 10: 已寄貨)
        /// </summary>
        [JsonPropertyName("Esmm_Status")]
        public int? EsmmStatus { get; set; }

        /// <summary>
        /// 物流單號
        /// </summary>
        [JsonPropertyName("Esmm_Ship_No")]
        public string? EsmmShipNo { get; set; }

        /// <summary>
        /// 寄送方式
        /// </summary>
        [JsonPropertyName("Esmm_Ship_Method")]
        public int? EsmmShipMethod { get; set; }

        /// <summary>
        /// 物流單號驗證碼
        /// </summary>
        [JsonPropertyName("Esmm_ShipNo_AuthCode")]
        public int? EsmmShipNoAuthCode { get; set; }

        /// <summary>
        /// 物流單號後綴
        /// </summary>
        [JsonPropertyName("Esmm_Ship_No_A")]
        public string? EsmmShipNoA { get; set; }

        /// <summary>
        /// IBON App 旗標
        /// </summary>
        [JsonPropertyName("Esmm_IbonApp_Flag")]
        public int? EsmmIbonAppFlag { get; set; }

        /// <summary>
        /// 貨態歷程清單 (管線字串格式: "狀態|日期,狀態|日期...")
        /// </summary>
        [JsonPropertyName("Esml_EsmmNo_List")]
        public string? EsmlEsmmNoList { get; set; }

        /// <summary>
        /// 物流狀態清單 (管線字串格式: "狀態碼|日期,狀態碼|日期...")
        /// </summary>
        [JsonPropertyName("Esms_EsmmNo_List")]
        public string? EsmsEsmmNoList { get; set; }

        /// <summary>
        /// 解析貨態歷程管線字串
        /// 格式: "01|2026-04-16T14:11:56.970,10|2026-04-16T14:20:00"
        /// </summary>
        /// <returns>貨態歷程清單</returns>
        public List<E_Shipment_L_Dto> ParseEsmlList()
        {
            return ParsePipelineString(EsmlEsmmNoList, (status, datetime) => new E_Shipment_L_Dto
            {
                EsmlEsmmStatus = status,
                EsmlStatusDatetime = datetime
            });
        }

        /// <summary>
        /// 解析物流狀態管線字串
        /// 格式: "1A01|2026-04-16T14:20:00,1001|2026-04-16T14:11:56.970"
        /// </summary>
        /// <returns>物流狀態清單</returns>
        public List<E_Shipment_S_Dto> ParseEsmsList()
        {
            return ParsePipelineString(EsmsEsmmNoList, (status, datetime) => new E_Shipment_S_Dto
            {
                EsmsDlvStatusNo = status,
                EsmsStatusDatetime = datetime
            });
        }

        /// <summary>
        /// 泛型管線字串解析器
        /// </summary>
        private static List<T> ParsePipelineString<T>(string? pipelineString, Func<string, DateTime, T> factory)
        {
            var result = new List<T>();

            if (string.IsNullOrWhiteSpace(pipelineString))
                return result;

            // 以逗號分隔每個項目
            var items = pipelineString.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in items)
            {
                // 以管線符號分隔狀態與日期
                var parts = item.Split('|');
                if (parts.Length != 2)
                    continue;

                var status = parts[0].Trim();
                var dateStr = parts[1].Trim();

                // 嘗試解析日期
                if (TryParseDateTime(dateStr, out var datetime))
                {
                    result.Add(factory(status, datetime));
                }
            }

            return result;
        }

        /// <summary>
        /// 嘗試解析日期字串 (支援多種格式)
        /// </summary>
        private static bool TryParseDateTime(string dateStr, out DateTime result)
        {
            result = default;

            // 支援的日期格式
            var formats = new[]
            {
                "yyyy-MM-ddTHH:mm:ss.fffZ",
                "yyyy-MM-ddTHH:mm:ss.fff",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd HH:mm:ss",
                "yyyyMMddHHmmss.fff",
                "yyyyMMddHHmmss"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    return true;
            }

            // 嘗試標準解析
            return DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result);
        }
    }

    /// <summary>
    /// 貨態歷程 DTO
    /// </summary>
    public class E_Shipment_L_Dto
    {
        /// <summary>
        /// 物流狀態 (01: 已取號, 10: 已寄貨)
        /// </summary>
        [JsonPropertyName("esml_esmm_status")]
        public string? EsmlEsmmStatus { get; set; }

        /// <summary>
        /// 狀態時間
        /// </summary>
        [JsonPropertyName("esml_status_datetime")]
        public DateTime? EsmlStatusDatetime { get; set; }
    }

    /// <summary>
    /// 物流狀態 DTO
    /// </summary>
    public class E_Shipment_S_Dto
    {
        /// <summary>
        /// 配送狀態碼 (例如: 1001, 1A01)
        /// </summary>
        [JsonPropertyName("esms_dlv_status_no")]
        public string? EsmsDlvStatusNo { get; set; }

        /// <summary>
        /// 狀態時間
        /// </summary>
        [JsonPropertyName("esms_status_datetime")]
        public DateTime? EsmsStatusDatetime { get; set; }
    }

    /// <summary>
    /// 物流主檔 DTO
    /// </summary>
    public class E_Shipment_M_Dto
    {
        [JsonPropertyName("esmm_no")]
        public string? EsmmNo { get; set; }

        [JsonPropertyName("esmm_ship_no")]
        public string? EsmmShipNo { get; set; }

        [JsonPropertyName("esmm_status")]
        public string? EsmmStatus { get; set; }

        [JsonPropertyName("esmm_ship_method")]
        public string? EsmmShipMethod { get; set; }

        [JsonPropertyName("esmm_ship_no_auth_code")]
        public string? EsmmShipNoAuthCode { get; set; }

        [JsonPropertyName("esmm_ship_no_a")]
        public string? EsmmShipNoA { get; set; }

        [JsonPropertyName("esmm_ibon_app_flag")]
        public string? EsmmIbonAppFlag { get; set; }
    }

    /// <summary>
    /// 訂單主檔 DTO
    /// </summary>
    public class C_Order_M_Dto
    {
        [JsonPropertyName("coom_status")]
        public string? CoomStatus { get; set; }
    }
}
