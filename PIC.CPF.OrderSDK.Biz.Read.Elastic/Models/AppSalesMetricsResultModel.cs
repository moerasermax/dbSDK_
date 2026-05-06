using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class AppSalesMetricsResultModel
    {
        /// <summary>
        /// 總銷售額：全部可出貨訂單的商品銷售額(含運費)
        /// </summary>
        [JsonPropertyName("totalAmount")]
        public int TotalAmount { get; set; }

        /// <summary>
        /// 總訂單數：排除異常單的總訂單數
        /// </summary>
        [JsonPropertyName("totalOrderCnt")]
        public int TotalOrderCnt { get; set; }

        /// <summary>
        /// 總寄件數：完成刷讀寄件的總數(包含取消寄件)
        /// </summary>
        [JsonPropertyName("shipmentsCnt")]
        public int ShipmentsCnt { get; set; }

        /// <summary>
        /// 總銷售額-環比：同TotalAmount，使用環比起迭日
        /// </summary>
        [JsonPropertyName("totalAmountPoP")]
        public int TotalAmountPoP { get; set; }

        /// <summary>
        /// 總訂單數-環比：同TotalOrderCnt，使用環比起迭日
        /// </summary>
        [JsonPropertyName("totalOrderCntPoP")]
        public int TotalOrderCntPoP { get; set; }

        /// <summary>
        /// 總寄件數-環比：同ShipmentsCnt，使用環比起迭日
        /// </summary>
        [JsonPropertyName("shipmentsCntPoP")]
        public int ShipmentsCntPoP { get; set; }

        /// <summary>
        /// 銷售額趨勢數據：以查詢類型為依據對訂單做Group By後加總訂單金額(含運費)
        /// </summary>
        [JsonPropertyName("salesTrendData")]
        public IEnumerable<OrderTrendData>? SalesTrendData { get; set; } = null;

        /// <summary>
        /// 訂單數趨勢數據：以查詢類型為依據對訂單做Group By後加總訂單數量
        /// </summary>
        [JsonPropertyName("orderTrendData")]
        public IEnumerable<OrderTrendData>? OrderTrendData { get; set; } = null;

        /// <summary>
        /// 商品銷售排名：以查詢類型為依據對訂單明細的cood_name做Group By後取出數量最多的前五名
        /// </summary>
        [JsonPropertyName("productSalesRanking")]
        public IEnumerable<ProductSalesRanking>? ProductSalesRanking { get; set; } = null;
    }

    public class OrderTrendData
    {
        /// <summary>
        /// 依照以下格式處理
        /// 本日     =>為24格: 01-24，格式為：HH
        /// 本週     =>為7格:  1/2/3/4/5/6/7，格式為：MM/dd
        /// 本月     =>為28-31格:  1-31，格式為：MM/dd
        /// 過去30天 =>為30格:格式為：MM/dd
        /// 按週     =>為7格:  1/2/3/4/5/6/7，格式為：MM/dd
        /// 按月     =>為28-31格:  1-31，格式為：MM/dd
        /// </summary>
        [JsonPropertyName("timePane")]
        public string? TimePane { get; set; } = null;

        /// <summary>
        /// Group By後算出的數字
        /// </summary>
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }

    public class ProductSalesRanking
    {
        /// <summary>
        /// 排名序號
        /// </summary>
        [JsonPropertyName("rankingNo")]
        public int RankingNo { get; set; }
        
        /// <summary>
        /// 賣場編號
        /// </summary>
        [JsonPropertyName("productCgdmid")]
        public string? ProductCgdmid { get; set; }
        
        /// <summary>
        /// 商品編號
        /// </summary>
        [JsonPropertyName("productId")]
        public string? ProductId { get; set; }
        
        /// <summary>
        /// 商品規格編號
        /// </summary>
        [JsonPropertyName("productCgdsId")]
        public string? ProductCgdsId { get; set; }
        
        /// <summary>
        /// 商品名稱
        /// </summary>
        [JsonPropertyName("productName")]
        public string? ProductName { get; set; } = null;
        
        /// <summary>
        /// 商品總銷售數
        /// </summary>
        [JsonPropertyName("productTotalSales")]
        public int ProductTotalSales { get; set; }
        
        /// <summary>
        /// 商品圖片
        /// </summary>
        [JsonPropertyName("productImgPath")]
        public string? ProductImgPath { get; set; } = null;
    }
}