
using Elastic.Clients.Elasticsearch;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL
{
    public partial class OrderSearchDal
    {
        public static Action<SortOptionsDescriptor<OrderDocument>> OrderInfoSort(OrderInfoSortModel model)
        {
            return s =>
            {
                if (model.OrderSorts != null)
                {
                    foreach (var orderSort in model.OrderSorts)
                    {
                        switch (orderSort)
                        {
                            case OrderSort.CoomCreateDatetimeAsc:
                                s.Field(f => f.CoomCreateDatetime, fs => fs.Order(SortOrder.Asc));
                                break;
                            case OrderSort.CoomCreateDatetimeDesc:
                                s.Field(f => f.CoomCreateDatetime, fs => fs.Order(SortOrder.Desc));
                                break;
                            case OrderSort.CoomNoAsc:
                                s.Field(f => f.CoomNo, fs => fs.Order(SortOrder.Asc));
                                break;
                            case OrderSort.CoomNoDesc:
                                s.Field(f => f.CoomNo, fs => fs.Order(SortOrder.Desc));
                                break;
                            case OrderSort.SellerQaNeverReplyCountAsc:
                                s.Field(f => f.SellerQaNeverReplyCount, fs => fs.Order(SortOrder.Asc));
                                break;
                            case OrderSort.SellerQaNeverReplyCountDesc:
                                s.Field(f => f.SellerQaNeverReplyCount, fs => fs.Order(SortOrder.Desc));
                                break;
                            case OrderSort.BuyerQaNeverReplyCountAsc:
                                s.Field(f => f.BuyerQaNeverReplyCount, fs => fs.Order(SortOrder.Asc));
                                break;
                            case OrderSort.BuyerQaNeverReplyCountDesc:
                                s.Field(f => f.BuyerQaNeverReplyCount, fs => fs.Order(SortOrder.Desc));
                                break;
                            default:
                                // 建議不要直接 throw，避免前端傳錯一個值就全掛
                                // 可以改為 continue;
                                continue;
                        }
                    }
                }

                // Tie-breaker：使用 coom_no（業務 unique key，已是 keyword 可 sort）。
                // 不能用 _id：ES 預設停用 _id 的 fielddata，sort by _id 會 search_phase_execution_exception。
                // 只在 user 沒指定 coom_no 排序時加，避免覆蓋使用者的方向設定。
                if (model.OrderSorts == null
                    || !model.OrderSorts.Any(o => o == OrderSort.CoomNoAsc || o == OrderSort.CoomNoDesc))
                {
                    s.Field(f => f.CoomNo, fs => fs.Order(SortOrder.Asc));
                }
            };
        }
    }
}
