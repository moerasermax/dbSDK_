using CPF.Services.Redis.Post.Model.MongoDB.Order;
using NO3._dbSDK_Imporve.Core.Entity;

namespace CPF.Services.Redis.Post.Model.QueryModel.MongoDB
{
    public class UpdateChangePayTypeEvent : Query
    {
        public ChangePayTypeEventArgs Args { get; set; } = null;
    }


    public class ChangePayTypeEventArgs
    {
       public string CoocNo { get; set; } = null;
       public C_Order_C_Model cooc { get;set;  }
    }
}
