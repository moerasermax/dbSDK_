using System;
using System.ComponentModel.DataAnnotations;

namespace CPF.Services.Redis.Post.Model
{
    public class AddOrdersList
    {
        public AddOrderArgs[] List { get; set; } = null!;
    }

    public class AddOrderArgs
    {
        public C_Order_M_CreateOrder coom { get; set; } = null!;

        public C_Order_C_CreateOrder cooc { get; set; } = null!;

        public C_Order_D_CreateOrder[] cood { get; set; } = null!;

        public C_Goods_Item_CreateOrder? cgdi { get; set; } = null;
    }

    public class C_Order_M_CreateOrder
    {
        [Required(ErrorMessage = "「CoomNo」是必填欄位")]
        [StringLength(15, MinimumLength = 15, ErrorMessage = "「CoomNo」必須是 15 個字元")]
        public string? CoomNo { get; set; } = string.Empty;

        //[Required(ErrorMessage = "「CoomOrderDate」是必填欄位")]
        public DateTime? CoomOrderDate { get; set; } = null;

        //[Required(ErrorMessage = "「CoomName」是必填欄位")]
        //[MaxLength(100, ErrorMessage = "「CoomName」長度不能超過 100 個字元")]
        public string? CoomName { get; set; } = null;

        //[Required(ErrorMessage = "「CoomStatus」是必填欄位")]
        //[RegularExpression("^10$", ErrorMessage = "「CoomStatus」必須是 '10'")]
        public string? CoomStatus { get; set; } = null;

        //[Required(ErrorMessage = "「CoomTempType」是必填欄位")]
        //[RegularExpression("^(01|02|03)$", ErrorMessage = "「CoomTempType」必須是 '01'、'02' 或 '03'")]
        public string? CoomTempType { get; set; } = null;

        //[Required(ErrorMessage = "「CoomCreateDatetime」是必填欄位")]
        public string? CoomCreateDatetime { get; set; } = null;

        //[Required(ErrorMessage = "「CoomCuamCid」是必填欄位")]
        //[Range(1, int.MaxValue, ErrorMessage = "「CoomCuamCid」必須大於 0")]
        public int? CoomCuamCid { get; set; } = null;

        //[Required(ErrorMessage = "「CoomSellerGoodsTotalAmt」是必填欄位")]
        //[Range(1, 20000, ErrorMessage = "「CoomSellerGoodsTotalAmt」必須在 1 到 20000 之間")]
        public int? CoomSellerGoodsTotalAmt { get; set; } = null;

        //[Required(ErrorMessage = "「CoomGoodsItemNum」是必填欄位")]
        //[Range(0, int.MaxValue, ErrorMessage = "「CoomGoodsItemNum」必須大於 0")]
        public int? CoomGoodsItemNum { get; set; } = null;

        //[Required(ErrorMessage = "「CoomGoodsTotalNum」是必填欄位")]
        //[Range(0, int.MaxValue, ErrorMessage = "「CoomGoodsTotalNum」必須大於 0")]
        public int? CoomGoodsTotalNum { get; set; } = null;

        //[Required(ErrorMessage = "「CoomRcvTotalAmt」是必填欄位")]
        //[Range(1, 20000, ErrorMessage = "「CoomRcvTotalAmt」必須在 1 到 20000 之間")]
        public int? CoomRcvTotalAmt { get; set; } = null;

        //[StringLength(15, MinimumLength = 15, ErrorMessage = "「CoomCgdmId」必須是 15 個字元")]
        public string? CoomCgdmId { get; set; } = null;
    }

    public class C_Order_C_CreateOrder
    {
        //[Required(ErrorMessage = "「CoocNo」是必填欄位")]
        //[StringLength(15, MinimumLength = 15, ErrorMessage = "「CoocNo」必須是 15 個字元")]
        public string? CoocNo { get; set; } = null;

        //[Required(ErrorMessage = "「CoocPaymentType」是必填欄位")]
        //[RegularExpression("^[0-6]$", ErrorMessage = "「CoocPaymentType」必須是 '0' 到 '6'")]
        public string? CoocPaymentType { get; set; } = null;

        //[Required(ErrorMessage = "「CoocDeliverMethod」是必填欄位")]
        //[RegularExpression("^[1-3]$", ErrorMessage = "「CoocDeliverMethod」必須是 '1' 到 '3'")]
        public string? CoocDeliverMethod { get; set; } = null;

        //[Required(ErrorMessage = "「CoocOrdChannelKind」是必填欄位")]
        //[RegularExpression("^[1-6]$", ErrorMessage = "「CoocOrdChannelKind」必須是 '1' 到 '6'")]
        public string? CoocOrdChannelKind { get; set; } = null;

        //[Required(ErrorMessage = "「CoocMemSid」是必填欄位")]
        //[Range(1, int.MaxValue, ErrorMessage = "「CoocMemSid」必須大於 0")]
        public int? CoocMemSid { get; set; } = null;

        //[Required(ErrorMessage = "「CoocCreateDatetime」是必填欄位")]
        public DateTime? CoocCreateDatetime { get; set; } = null;

        //[Required(ErrorMessage = "「CoocOrdNameEnc」是必填欄位")]
        public string? CoocOrdNameEnc { get; set; } = null;

        //[Required(ErrorMessage = "「CoocRcvNameEnc」是必填欄位")]
        public string? CoocRcvNameEnc { get; set; } = null;

        //[Required(ErrorMessage = "「CoocRcvMobileEnc」是必填欄位")]
        public string? CoocRcvMobileEnc { get; set; } = null;

        public DateTime? CoocPaymentPayDatetime { get; set; } = null;
        public DateTime? CoocPaymentDueday { get; set; } = null;
    }

    public class C_Order_D_CreateOrder
    {
        public string? CgddId { get; set; } = null!;

        public string? CoodCgdsId { get; set; } = null!;

        //[Required(ErrorMessage = "「CoodName」是必填欄位")]
        //[MaxLength(100, ErrorMessage = "「CoodName」長度不能超過 100 個字元")]
        public string CoodName { get; set; } = null!;

        //[Required(ErrorMessage = "「CoodQty」是必填欄位")]
        //[Range(1, int.MaxValue, ErrorMessage = "「CoodQty」必須大於 0")]
        public int? CoodQty { get; set; } = null;

        //[Required(ErrorMessage = "「CoodOriginalPrice」是必填欄位")]
        //[Range(1, 20000, ErrorMessage = "「CoodOriginalPrice」必須在 1 到 20000 之間")]
        public int? CoodOriginalPrice { get; set; } = null;

        public int? CoodDiscountPrice { get; set; } = null;

        //[Required(ErrorMessage = "「CoodReceivePrice」是必填欄位")]
        //[Range(1, 20000, ErrorMessage = "「CoodReceivePrice」必須在 1 到 20000 之間")]
        public int? CoodReceivePrice { get; set; } = null;

        //[MaxLength(200, ErrorMessage = "「CoodImagePath」長度不能超過 200 個字元")]
        public string? CoodImagePath { get; set; } = null;
    }

    public class C_Goods_Item_CreateOrder
    {
        public int? CgdiNumberColumn1 { get; set; } = null;
    }
}
