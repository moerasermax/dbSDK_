
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    public class OrderInfoSortModel
    {
        /// <summary>
        /// 排序條件
        /// </summary>
        public OrderSort[]? OrderSorts { get; set; } = null;
    }
}
