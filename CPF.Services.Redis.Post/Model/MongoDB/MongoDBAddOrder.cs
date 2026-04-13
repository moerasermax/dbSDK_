using CPF.Services.Redis.Post.Model.MongoDB.AddOrderEvent.Order;
using NO3._dbSDK_Imporve.Core.Entity;

namespace CPF.Services.Redis.Post.Model.MongoDB
{
    public class MongoDBAddOrder : Query
    {
        public OrderArgs? Args { get; set; } = null;
    }
    public class OrderArgs
    {
        public string? CoomNo { get; set; }
        public string? CoocNo { get; set; }
        public C_Order_M_Model coom { get; set; } = new();
        public C_Order_C_Model cooc { get; set; } = new();
        public List<C_Order_D_Model> cood { get; set; } = new();
        public C_Goods_Item_Model? cgdi { get; set; }
        public List<string>? CoomNoList { get; set; }
       
    }
}
