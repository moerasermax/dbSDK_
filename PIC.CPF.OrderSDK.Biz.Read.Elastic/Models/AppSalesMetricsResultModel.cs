namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class AppSalesMetricsResultModel
    {
        /// <summary>
        /// 總銷售額：全部可出貨訂單的商品銷售額(含運費)
        /// </summary>
        public int TotalAmount { get; set; }

        /// <summary>
        /// 總訂單數：排除異常單的總訂單數
        /// </summary>
        public int TotalOrderCnt { get; set; }

        /// <summary>
        /// 總寄件數：完成刷讀寄件的總數(包含取消寄件)
        /// </summary>
        public int ShipmentsCnt { get; set; }

        /// <summary>
        /// 總銷售額-環比：同TotalAmount，使用環比起迄日
        /// </summary>
        public int TotalAmountPoP { get; set; }

        /// <summary>
        /// 總訂單數-環比：同TotalOrderCnt，使用環比起迄日
        /// </summary>
        public int TotalOrderCntPoP { get; set; }

        /// <summary>
        /// 總寄件數-環比：同ShipmentsCnt，使用環比起迄日
        /// </summary>
        public int ShipmentsCntPoP { get; set; }

        /// <summary>
        /// 銷售額趨勢數據：以查詢類型為依據對訂單做Group By後加總訂單金額(含運費)
        /// </summary>
        public IEnumerable<OrderTrendData>? SalesTrendData { get; set; } = null;

        /// <summary>
        /// 訂單數趨勢數據：以查詢類型為依據對訂單做Group By後加總訂單數量
        /// </summary>
        public IEnumerable<OrderTrendData>? OrderTrendData { get; set; } = null;

        /// <summary>
        /// 商品銷售排名：以查詢類型為依據對訂單明細的cood_name做Group By後取出數量最多的前五名
        /// </summary>
        public IEnumerable<ProductSalesRanking>? ProductSalesRanking { get; set; } = null;

        /// <summary>
        /// 查詢時間
        /// </summary>
        public long Took { get; set; }
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
        public string? TimePane { get; set; } = null;

        /// <summary>
        /// Group By後算出的數字
        /// </summary>
        public int Value { get; set; }
    }

    public class ProductSalesRanking
    {
        /// <summary>
        /// 排名序號
        /// </summary>
        public int RankingNo { get; set; }
        /// <summary>
        /// 賣場編號
        /// </summary>
        public string? ProductCgdmid { get; set; }
        /// <summary>
        /// 商品編號
        /// </summary>
        public string? ProductId { get; set; }
        /// <summary>
        /// 商品規格編號
        /// </summary>
        public string? ProductCgdsId { get; set; }
        /// <summary>
        /// <summary>
        /// 商品名稱
        /// </summary>
        public string? ProductName { get; set; } = null;
        /// <summary>
        /// 商品總銷售數
        /// </summary>
        public int ProductTotalSales { get; set; }
        /// <summary>
        /// 商品圖片
        /// </summary>
        public string? ProductImgPath { get; set; } = null;
    }
}
