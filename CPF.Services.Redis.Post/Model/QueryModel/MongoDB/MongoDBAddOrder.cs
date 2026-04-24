using AddOrderEventOrder = CPF.Services.Redis.Post.Model.MongoDB.AddOrderEvent.Order;
using NO3Entity = NO3._dbSDK_Imporve.Core.Entity;

namespace CPF.Services.Redis.Post.Model.QueryModel.MongoDB
{
    public class MongoDBAddOrder : NO3Entity.Query
    {
        public OrderArgs? Args { get; set; } = null;
    }
    public class OrderArgs
    {
        public string? CoomNo { get; set; }
        public string? CoocNo { get; set; }
        public AddOrderEventOrder.C_Order_M_Model coom { get; set; } = new();
        public AddOrderEventOrder.C_Order_C_Model cooc { get; set; } = new();
        public List<AddOrderEventOrder.C_Order_D_Model> cood { get; set; } = new();
        public AddOrderEventOrder.C_Goods_Item_Model? cgdi { get; set; }
        public List<string>? CoomNoList { get; set; }
       
    }
}
