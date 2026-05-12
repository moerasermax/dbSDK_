
// 定義外部傳入的公用 Model (假設你目前還是共用 OpenSearch 的外部 Model)
// 如果你已經把外部 Model 也搬到 Elastic 目錄下，請將這行改為 PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
using PublicModels = PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

// 定義新版 Elastic 專用的內部 Model
using InternalModels = PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;
using System.Text.Json;

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
                BuyerOverview = model.BuyerOverview != null ? new PublicModels.BuyerOverviewAggregateResultModel
                {
                    Unpaid = model.BuyerOverview.Unpaid,
                    Toship = model.BuyerOverview.Toship,
                    ToFinish = model.BuyerOverview.ToFinish,
                    Cancel = model.BuyerOverview.Cancel,
                    BuyerQaNeverReply = model.BuyerOverview.SellerQaNeverReply,
                    Finish = model.BuyerOverview.Finish,
                    BuyerReturnReq = model.BuyerOverview.BuyerReturnReq,
                } : null,
                BuyerPerformance = model.BuyerPerformance != null ? new PublicModels.BuyerPerformanceAggregateResultModel
                {
                    OrderCount = model.BuyerPerformance.OrderCount,
                    PickupCount = model.BuyerPerformance.PickupCount,
                } : null,
                SellerOverview = model.SellerOverview != null ? new PublicModels.SellerOverviewAggregateResultModel
                {
                    DealWith = model.SellerOverview.DealWith,
                    Toship = model.SellerOverview.Toship,
                    Shipping = model.SellerOverview.Shipping,
                    NoShowToDHL = model.SellerOverview.WaitReturn,
                    SellerQaNeverReply = model.SellerOverview.BuyerQaNeverReply,
                    SellerReturnReq = model.SellerOverview.SellerReturnReq,
                } : null,
                SellerPerformance = model.SellerPerformance != null ? new PublicModels.SellerPerformanceAggregateResultModel
                {
                    OrderCount = model.SellerPerformance.OrderCount,
                    SendCount = model.SellerPerformance.SendCount,
                    SalesAmt = model.SellerPerformance.SalesAmt,
                } : null,
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
            var orderInfos = model.Documents
                .Select(ConvertToSearchOrderInfoDataModel)
                .Where(d => d is not null)
                .Select(d => d!)
                .ToArray();

            return new PublicModels.SearchOrderInfoResultModel
            {
                OrderInfos = orderInfos.Length > 0 ? orderInfos : null,
                Total = model.Total,
            };
        }

        private static PublicModels.SearchOrderInfoDataModel? ConvertToSearchOrderInfoDataModel(JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Null || je.ValueKind == JsonValueKind.Undefined)
                return null;

            InternalModels.OrderDocument? doc;
            try
            {
                doc = JsonSerializer.Deserialize<InternalModels.OrderDocument>(je.GetRawText());
            }
            catch (JsonException)
            {
                return null;
            }
            if (doc is null) return null;

            return new PublicModels.SearchOrderInfoDataModel
            {
                COrderM = MapOrderMaster(doc),
                COrderC = MapOrderCart(doc),
                COrderD = MapOrderItems(doc),
                CGoodsItem = new PublicModels.GoodsItemModel(),
                EShipmentM = MapShipmentMaster(doc),
                CQuestionM = MapQuestionMaster(doc),
                CCancelM = new PublicModels.CancelMasterModel(),
                EShipmentL = null,
                EShipmentS = null,
                ECCDHL = new PublicModels.CCDHLModel(),
                ECCCS = new PublicModels.CCCSModel(),
                ERtnDHLApply = new PublicModels.RtnDHLApplyModel(),
            };
        }

        private static PublicModels.OrderMasterModel MapOrderMaster(InternalModels.OrderDocument doc) => new()
        {
            CoomNo = doc.CoomNo,
            CoomOrderDate = null,
            CoomName = doc.CoomName,
            CoomStatus = doc.CoomStatus,
            CoomTempType = doc.CoomTempType,
            CoomCreateDatetime = doc.CoomCreateDatetime,
            CoomCuamCid = doc.CoomCuamCid,
            CoomReChoiceFlag = null,
            CoomMergeListCoomNo = null,
            CoomSellerMemo = null,
            CoomSellerGoodsTotalAmt = null,
            CoomGoodsItemNum = null,
            CoomGoodsTotalNum = null,
            CoomRcvTotalAmt = doc.CoomRcvTotalAmt,
            CoomCgdmId = null,
            CoomShipPrintFlag = null,
            CoomCccmNo = null,
        };

        private static PublicModels.OrderCartModel MapOrderCart(InternalModels.OrderDocument doc) => new()
        {
            CoocNo = doc.CoocNo,
            CoocPaymentType = doc.CoocPaymentType,
            CoocPaymentPayDatetime = doc.CoocPaymentPayDatetime,
            CoocDeliverMethod = doc.CoocDeliverMethod,
            CoocOrdChannelKind = doc.CoocOrdChannelKind,
            CoocCreateDatetime = null,
            CoocMemSid = doc.CoocMemSid,
            CoocPaymentCode = null,
            CoocOrdNameEnc = null,
            CoocRcvNameEnc = null,
            CoocRcvMobileEnc = null,
            CoocPaymentTradeNo = null,
            CoocPaymentNote = null,
            CoocPaymentBankCode = null,
            CoocPaymentDueday = null,
        };

        private static PublicModels.OrderItemModel[]? MapOrderItems(InternalModels.OrderDocument doc)
        {
            if (doc.CoodItems is { Length: > 0 } items)
            {
                return items.Select(ci => new PublicModels.OrderItemModel
                {
                    CoodName = ci.CoodName,
                    CoodQty = ci.CoodQty,
                    CoodOriginalPrice = null,
                    CoodDiscountPrice = null,
                    CoodReceivePrice = null,
                    CoodImagePath = ci.CoodImagePath,
                }).ToArray();
            }
            if (doc.CoodNames is { Length: > 0 } names)
            {
                return names.Select(n => new PublicModels.OrderItemModel { CoodName = n }).ToArray();
            }
            return null;
        }

        private static PublicModels.ShipmentMasterModel MapShipmentMaster(InternalModels.OrderDocument doc) => new()
        {
            EsmmNo = null,
            EsmmShipNo = doc.EsmmShipNo,
            EsmmStatus = doc.EsmmStatus,
            EsmmShipMethod = null,
            EsmmShipNoAuthCode = null,
            EsmmShipNoA = null,
            EsmmLeaveStoreDateB = doc.EsmmLeaveStoreDateB,
            EsmmIbonAppFlag = null,
            EsmmOddReason = null,
            EsmmConfirmExtpayDatetime = null,
        };

        private static PublicModels.QuestionMasterModel MapQuestionMaster(InternalModels.OrderDocument doc) => new()
        {
            SellerQaNeverReplyCount = doc.SellerQaNeverReplyCount,
            BuyerQaNeverReplyCount = doc.BuyerQaNeverReplyCount,
        };
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
                AppSellerOverView = model.AppDashboard != null ? new PublicModels.AppSellerOverViewAggregateResultModel
                {
                    NewOrderCnt = model.AppDashboard.NewOrderCnt,
                    ShippedCnt = model.AppDashboard.ShippedCnt,
                    RepliedCnt = model.AppDashboard.RepliedCnt,
                    PickupCnt = model.AppDashboard.PickupCnt
                } : null,
                AppSellerPerformance = model.AppPerformance != null ? new PublicModels.AppSellerPerformanceAggregateResultModel
                {
                    SalesAmount = model.AppPerformance.SalesAmount,
                    TotalOrderQty = model.AppPerformance.TotalOrderQty,
                } : null,
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
            }).ToArray();
        }

        /// <summary>
        /// 為趨勢資料補零、確保完整 time-slot 序列。
        /// - Today: 固定 24 格 (01〜24 小時)、不參考 searchStart/end
        /// - 其他類型 (ThisWeek/ThisMonth/SetWeek/SetMonth/PastOneMonth):
        ///     用 searchStartDate ~ searchEndDate 區間 (本地時區 Date 部分) 生成每日序列
        ///     對齊 Golden Search 6: 8 天區間 → 8 筆 trend
        /// 修正點 (S41-F):
        ///   1. 原用 `DateTime.Now` 算當週週一起 7 天、與使用者傳入的 searchStart/end 完全脫鉤
        ///   2. 原硬編 expectedCount=7、忽略實際區間長度 (8 天區間被截成 7 格)
        ///   3. ThisMonth/SetMonth 原直接 return 不補、改為依 req 區間補日序列
        /// </summary>
        internal static PublicModels.AppSalesMetricsResultModel ApplyZeroPadding(
            this PublicModels.AppSalesMetricsResultModel model,
            DateRangeType? dateRangeType,
            DateTime? searchStartDate = null,
            DateTime? searchEndDate = null)
        {
            if (dateRangeType == null) return model;

            if (dateRangeType == DateRangeType.Today)
            {
                if (model.SalesTrendData != null)
                    model.SalesTrendData = PadTrendDataHourly(model.SalesTrendData.ToList());
                if (model.OrderTrendData != null)
                    model.OrderTrendData = PadTrendDataHourly(model.OrderTrendData.ToList());
                return model;
            }

            // Daily 路徑 (Week / Month / PastOneMonth + 對應 Set 版本)
            // 一律以 searchStart~searchEnd 為趨勢區間;無 searchStart/end 則不補避免錯位
            if (searchStartDate is null || searchEndDate is null) return model;

            var start = searchStartDate.Value.ToLocalTime().Date;
            var end = searchEndDate.Value.ToLocalTime().Date;
            if (start > end) return model;

            if (model.SalesTrendData != null)
                model.SalesTrendData = PadTrendDataDaily(model.SalesTrendData.ToList(), start, end);
            if (model.OrderTrendData != null)
                model.OrderTrendData = PadTrendDataDaily(model.OrderTrendData.ToList(), start, end);

            return model;
        }

        // Hour 序列 00-23 (對齊 ES DateHistogram Format("HH") + Golden Search_5 樣張 timePane "00"~"23")
        // 原 01-24 是 bug、與 ES 回傳 "HH" (00-23) 不匹配、所有 hour bucket lookup fail、整個 trend 全 0
        private static List<PublicModels.OrderTrendData> PadTrendDataHourly(
            List<PublicModels.OrderTrendData> existingData)
        {
            var result = new List<PublicModels.OrderTrendData>();
            for (int i = 0; i <= 23; i++)
            {
                var hourStr = i.ToString("D2");
                var existing = existingData.FirstOrDefault(d => d.TimePane == hourStr);
                result.Add(new PublicModels.OrderTrendData
                {
                    TimePane = hourStr,
                    Value = existing?.Value ?? 0
                });
            }
            return result;
        }

        private static List<PublicModels.OrderTrendData> PadTrendDataDaily(
            List<PublicModels.OrderTrendData> existingData, DateTime start, DateTime end)
        {
            var result = new List<PublicModels.OrderTrendData>();
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                var dateStr = d.ToString("MM/dd");
                var existing = existingData.FirstOrDefault(t => t.TimePane == dateStr);
                result.Add(new PublicModels.OrderTrendData
                {
                    TimePane = dateStr,
                    Value = existing?.Value ?? 0
                });
            }
            return result;
        }
        #endregion

        // ==========================================
        // Flow 4: Search 7 GetUserCgdmData (legacy ES 聚合路徑、S41-E 後不再被 BLL 呼叫,保留為審計 trail)
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

        // ==========================================
        // Flow 4 (S41-E): Search 7 Mongo 單筆路徑
        // mirror 客戶原 SDK: result.Data = ConvertToExtension.ConvertToUserData(DDBUser)
        // Mongo cgdm_update_datetime 為 UTC (BSON Date)、反序列化後 DateTime.Kind=Unspecified;
        // 此處先 SpecifyKind Utc 再 ConvertTimeFromUtc → Asia/Taipei、輸出無 Z 後綴對齊 Golden Recipe
        // ==========================================
        private static readonly TimeZoneInfo TaipeiTz = ResolveTaipeiTz();

        private static TimeZoneInfo ResolveTaipeiTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei"); }
            catch (TimeZoneNotFoundException) { }
            try { return TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"); }
            catch (TimeZoneNotFoundException) { }
            return TimeZoneInfo.CreateCustomTimeZone("Asia/Taipei", TimeSpan.FromHours(8), "Asia/Taipei", "Asia/Taipei");
        }

        internal static PublicModels.UserCgdmDataResultModel ConvertToUserCgdmDataResultModel(
            this InternalModels.MongoUser? user, int cuamCid)
        {
            if (user?.CGoodsM is null || user.CGoodsM.Count == 0)
            {
                return new PublicModels.UserCgdmDataResultModel
                {
                    CuamCid = cuamCid,
                    Cgdm = Array.Empty<PublicModels.CgdmDataModel>(),
                };
            }

            return new PublicModels.UserCgdmDataResultModel
            {
                CuamCid = cuamCid,
                Cgdm = user.CGoodsM.Select(g => new PublicModels.CgdmDataModel
                {
                    CgdmId = g.CgdmId ?? string.Empty,
                    CgdmUpdateDatetime = FormatTaipeiNoZ(g.CgdmUpdateDatetime),
                }).ToArray(),
            };
        }

        // UTC → Asia/Taipei (UTC+8)、yyyy-MM-ddTHH:mm:ss.fff、無 Z 後綴
        private static string FormatTaipeiNoZ(DateTime? utcDate)
        {
            if (!utcDate.HasValue) return string.Empty;
            var utc = DateTime.SpecifyKind(utcDate.Value, DateTimeKind.Utc);
            var taipei = TimeZoneInfo.ConvertTimeFromUtc(utc, TaipeiTz);
            return taipei.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }
    }
}