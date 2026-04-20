using NO3._dbSDK_Imporve.Core.Entity;
using CPF.Services.Redis.Post.Model.MongoDB.Order;
namespace CPF.Services.Redis.Post.Model.QueryModel.MongoDB
{
    public class UpdateCoomSellerMemo : Query
    {
        public SellerMemoArgs? Args { get; set; } = null;
    }
    public class SellerMemoArgs
    {
        //[Required(ErrorMessage = "「CoomNo」是必填欄位")]
        //[StringLength(15, MinimumLength = 15, ErrorMessage = "「CoomNo」必須是 15 個字元")]
        public string? CoomNo { get; set; } = null;

        //[Required(ErrorMessage = "「CoomSellerMemo」是必填欄位")]
        //[MaxLength(200, ErrorMessage = "「CoomSellerMemo」長度不能超過 200 個字元")]
        public C_Order_M_Model? coom { get; set; } = null;
    }
}
