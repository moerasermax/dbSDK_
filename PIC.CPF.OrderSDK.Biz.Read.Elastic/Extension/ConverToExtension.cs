
// 定義外部傳入的公用 Model (假設你目前還是共用 OpenSearch 的外部 Model)
// 如果你已經把外部 Model 也搬到 Elastic 目錄下，請將這行改為 PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
using PublicModels = PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

// 定義新版 Elastic 專用的內部 Model
using InternalModels = PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Extension
{
    internal static class ConvertToExtension
    {

        // ==========================================
        // Flow 1: Web 統計相關轉換
        // ==========================================
        internal static InternalModels.BuyerOverviewAggregateModel[]? ConvertToBuyerOverviewAggregateModel(this PublicModels.AggregateOrderInfoModel model)
        {
            return model.BuyerOverview?.Select(b => new InternalModels.BuyerOverviewAggregateModel
            {
                CoocMemSid = b.CoocMemSid,
                OrderDateEnd = b.OrderDateEnd,
                OrderDateStart = b.OrderDateStart,
            })?.ToArray();
        }

        internal static InternalModels.BuyerPerformanceAggregateModel[]? ConvertToBuyerPerformanceAggregateModel(this PublicModels.AggregateOrderInfoModel model)
        {
            return model.BuyerPerformance?.Select(b => new InternalModels.BuyerPerformanceAggregateModel
            {
                CoocMemSid = b.CoocMemSid,
                OrderDateEnd = b.OrderDateEnd,
                OrderDateStart = b.OrderDateStart,
            }).ToArray();
        }

        internal static InternalModels.SellerOverviewAggregateModel[]? ConvertToSellerOverviewAggregateModel(this PublicModels.AggregateOrderInfoModel model)
        {
            return model.SellerOverview?.Select(s => new InternalModels.SellerOverviewAggregateModel
            {
                CoomCuamCid = s.CoomCuamCid,
                OrderDateEnd = s.OrderDateEnd,
                OrderDateStart = s.OrderDateStart,
            }).ToArray();
        }

        internal static InternalModels.SellerPerformanceAggregateModel[]? ConvertToSellerPerformanceAggregateModel(this PublicModels.AggregateOrderInfoModel model)
        {
            return model.SellerPerformance?.Select(s => new InternalModels.SellerPerformanceAggregateModel
            {
                CoomCuamCid = s.CoomCuamCid,
                OrderDateEnd = s.OrderDateEnd,
                OrderDateStart = s.OrderDateStart,
            }).ToArray();
        }

        internal static PublicModels.AggregateOrderInfoResultModel ConvertToAggregateOrderInfoResultModel(this InternalModels.AggregateOrderInfoResultModel model)
        {
            return new PublicModels.AggregateOrderInfoResultModel
            {
                BuyerOverview = model.BuyerOverview?.Select(b => new PublicModels.BuyerOverviewAggregateResultModel
                {
                    Unpaid = b.Unpaid,
                    Toship = b.Toship,
                    ToFinish = b.ToFinish,
                    Cancel = b.Cancel,
                    BuyerQaNeverReply = b.SellerQaNeverReply,
                    Finish = b.Finish,
                    BuyerReturnReq = b.BuyerReturnReq,
                })?.ToArray(),
                BuyerPerformance = model.BuyerPerformance?.Select(b => new PublicModels.BuyerPerformanceAggregateResultModel
                {
                    OrderCount = b.OrderCount,
                    PickupCount = b.PickupCount,
                })?.ToArray(),
                SellerOverview = model.SellerOverview?.Select(b => new PublicModels.SellerOverviewAggregateResultModel
                {
                    DealWith = b.DealWith,
                    Toship = b.Toship,
                    Shipping = b.Shipping,
                    NoShowToDHL = b.WaitReturn,
                    SellerQaNeverReply = b.BuyerQaNeverReply,
                    SellerReturnReq = b.SellerReturnReq,
                })?.ToArray(),
                SellerPerformance = model.SellerPerformance?.Select(b => new PublicModels.SellerPerformanceAggregateResultModel
                {
                    OrderCount = b.OrderCount,
                    SendCount = b.SendCount,
                    SalesAmt = b.SalesAmt,
                })?.ToArray(),
                Took = model.Took,
            };
        }
        // ==========================================
        // Flow 2: 訂單搜尋相關轉換
        // ==========================================
        internal static InternalModels.OrderInfoQueryModel ConverToOrderInfoQueryModel(this PublicModels.SearchOrderInfoModel model)
        {
            return new InternalModels.OrderInfoQueryModel
            {
                CoocMemSid = model.CoocMemSid,
                CoodName = model.CoodName,
                CoocNo = model.CoocNo,
                CoomName = model.CoomName,
                CoomNo = model.CoomNo,
                DeliverMethodSearchKind = model.DeliverMethodSearchKind,
                EsmmShipNo = model.EsmmShipNo,
                OrderDateEnd = model.OrderDateEnd,
                OrderDateStart = model.OrderDateStart,
                OrderState = model.OrderState,
                OrdChannelKindSearchKind = model.OrdChannelKindSearchKind,
                CoomCuamCid = model.CoomCuamCid,
                TempTypeSearchKind = model.TempTypeSearchKind,
                IsQaList = model.IsQaList,
                BindMembersArray = model.BindMembersArray
            };
        }

        internal static InternalModels.OrderInfoSortModel ConvertToOrderInfoSortModel(this PublicModels.SearchOrderInfoModel model)
        {
            return new InternalModels.OrderInfoSortModel
            {
                OrderSorts = model.OrderSorts,
            };
        }

        internal static PublicModels.SearchOrderInfoResultModel ConvertToSearchOrderInfoResultModel(this InternalModels.SearchOrderInfoResultModel model)
        {
            return new PublicModels.SearchOrderInfoResultModel
            {
                OrderInfos = model.Documents,
                Total = model.Total,
                Took = model.Took,
            };
        }
        #region ESAPP
        // ==========================================
        // Flow 3: App 統計相關轉換
        // ==========================================
        internal static InternalModels.AppSellerOverViewAggregateModel[]? ConvertToAppSellerOverviewAggregateModel(this PublicModels.AppAggregateOrderInfoModel model)
        {
            return model.AppSellerOverView?.Select(b => new InternalModels.AppSellerOverViewAggregateModel
            {
                CoomCuamCid = b.CoomCuamCid,
                OrderDateEnd = b.OrderDateEnd,
                OrderDateStart = b.OrderDateStart,
            })?.ToArray();
        }

        internal static InternalModels.AppSellerPerformanceAggregateModel[]? ConvertToAppSellerPerformanceAggregateModel(this PublicModels.AppAggregateOrderInfoModel model)
        {
            return model.AppSellerPerformance?.Select(b => new InternalModels.AppSellerPerformanceAggregateModel
            {
                CoomCuamCid = b.CoomCuamCid,
                OrderDateEnd = b.OrderDateEnd,
                OrderDateStart = b.OrderDateStart,
            })?.ToArray();
        }

        internal static PublicModels.AppDashboardAggregateResultModel ConvertToAppDashboardAggregateResultModel(this InternalModels.AppDashboardAggregateResultModel model)
        {
            return new PublicModels.AppDashboardAggregateResultModel
            {
                appSellerOverView = model.AppDashboard?.Select(b => new PublicModels.AppSellerOverViewAggregateResultModel
                {
                    NewOrderCnt = b.NewOrderCnt,
                    ShippedCnt = b.ShippedCnt,
                    RepliedCnt = b.RepliedCnt,
                    PickupCnt = b.PickupCnt
                })?.ToArray(),
                appSellerPerformance = model.AppPerformance?.Select(b => new PublicModels.AppSellerPerformanceAggregateResultModel
                {
                    SalesAmount = b.SalesAmount,
                    TotalOrderQty = b.TotalOrderQty,
                })?.ToArray(),
                Took = model.Took,
            };
        }

        internal static InternalModels.AppSalesMetricsModel[]? ConvertToAppSalesMetricsModel(this PublicModels.AppSalesMetricsModel[] model)
        {
            return model.Select(b => new InternalModels.AppSalesMetricsModel
            {
                CuamCid = b.CuamCid,
                SearchStartDate = b.SearchStartDate,
                SearchEndDate = b.SearchEndDate,
                StartDatePoP = b.StartDatePoP,
                EndDatePoP = b.EndDatePoP,
                DateRangeType = b.DateRangeType
            })?.ToArray();
        }

        internal static PublicModels.AppSalesMetricsResultModel[] ConvertToAppSalesMetricsResultModel(this InternalModels.AppSalesMetricsResultModel[] model)
        {
            return model.Select(b => new PublicModels.AppSalesMetricsResultModel
            {
                TotalAmount = b.TotalAmount,
                TotalOrderCnt = b.TotalOrderCnt,
                ShipmentsCnt = b.ShipmentsCnt,
                TotalAmountPoP = b.TotalAmountPoP,
                TotalOrderCntPoP = b.TotalOrderCntPoP,
                ShipmentsCntPoP = b.ShipmentsCntPoP,
                SalesTrendData = b.SalesTrendData?.Select(s => new PublicModels.OrderTrendData
                {
                    TimePane = s.TimePane,
                    Value = s.Value
                }),
                OrderTrendData = b.OrderTrendData?.Select(o => new PublicModels.OrderTrendData
                {
                    TimePane = o.TimePane,
                    Value = o.Value
                }),
                ProductSalesRanking = b.ProductSalesRanking?.Select(p => new PublicModels.ProductSalesRanking
                {
                    RankingNo = p.RankingNo,
                    ProductCgdmid = p.ProductCgdmid,
                    ProductId = p.ProductId,
                    ProductCgdsId = p.ProductCgdsId,
                    ProductName = p.ProductName,
                    ProductTotalSales = p.ProductTotalSales,
                    ProductImgPath = p.ProductImgPath
                }),
                Took = b.Took,
            }).ToArray();
        }
        #endregion

        // ==========================================
        // Flow 4: Search 7 GetUserCgdmData
        // ==========================================
        internal static PublicModels.UserCgdmDataResultModel ConvertToUserCgdmDataResultModel(
            this InternalModels.UserCgdmDataAggregateModel[] models, int cuamCid)
        {
            return new PublicModels.UserCgdmDataResultModel
            {
                CuamCid = cuamCid,
                Cgdm = models.Select(m => new PublicModels.CgdmDataModel
                {
                    CgdmId = m.CgdmId,
                    CgdmUpdateDatetime = m.MaxModifyDate?.ToString("yyyy-MM-ddTHH:mm:ss.fff") ?? string.Empty,
                }).ToArray(),
            };
        }
    }
}