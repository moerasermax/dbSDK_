using MongoOrder = CPF.Services.Redis.Post.Model.MongoDB.Order;
using NO3Entity = NO3._dbSDK_Imporve.Core.Entity;

namespace CPF.Services.Redis.Post.Model.QueryModel.MongoDB
{
    public class UpdateChangePayTypeEvent : NO3Entity.Query
    {
        public ChangePayTypeEventArgs Args { get; set; } = null;
    }


    public class ChangePayTypeEventArgs
    {
       public string CoocNo { get; set; } = null;
       public MongoOrder.C_Order_C_Model cooc { get;set;  }
    }
}
