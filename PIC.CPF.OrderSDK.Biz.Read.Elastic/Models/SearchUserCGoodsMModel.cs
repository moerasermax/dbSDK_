namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    /// <summary>
    /// Search 7 (GetUserCgdmData) Public Input Model。
    /// 對齊客戶 Controller `[HttpPost("GetUserCgdmData")] GetUserCgdmData(SearchUserCGoodsMModel model)`。
    /// 依 Search_7 In 樣張:cuamCid。
    /// SDK 內部:BLL 用 cuamCid 從 Mongo Users collection 單筆 LoadAsync (S41-E 改採 Mongo)、回 cgdm 陣列。
    /// </summary>
    public class SearchUserCGoodsMModel
    {
        public int? CuamCid { get; init; }
    }
}
