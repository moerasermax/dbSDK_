using CPF.Services.Redis.Post.Model.MongoDB.Order;
using NO3._dbSDK_Imporve.Core.Entity;
using C_Order_M_Model = CPF.Services.Redis.Post.Model.MongoDB.Order.C_Order_M_Model;
using E_Shipment_M_Model = CPF.Services.Redis.Post.Model.MongoDB.Order.E_Shipment_M_Model;
using E_Shipment_L_Model = CPF.Services.Redis.Post.Model.MongoDB.Order.E_Shipment_L_Model;
using E_Shipment_S_Model = CPF.Services.Redis.Post.Model.MongoDB.Order.E_Shipment_S_Model;

namespace CPF.Services.Redis.Post.Model.QueryModel.MongoDB
{
    /// <summary>
    /// 取號事件 (UpdateSellerGetNumberEvent)
    /// </summary>
    public class UpdateSellerGetNumberEvent : Query
    {
        public SellerGetNumberArgs? Args { get; set; } = null;
    }

    public class SellerGetNumberArgs
    {
        public string? CoomNo { get; set; }
        public C_Order_M_Model? coom { get; set; }
        public E_Shipment_M_Model? esmm { get; set; }
        public List<E_Shipment_L_Model>? esml { get; set; }
        public List<E_Shipment_S_Model>? esms { get; set; }
    }
}