using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Extension
{
    // Dual Engine 流程的 Mongo → Public Model 轉換層
    // mirror 客戶原 SDK: result.Data = ConvertToExtension.ConvertToOrderData(DDBData);
    // dbSDK caller: var orderInfos = ddbData.ConvertToOrderData();
    internal static class MongoToOrderDataExtension
    {
        // dbSDK 全局註冊 MultiCultureDateTimeSerializer (NO3._dbSDK_Imporve.MongoMap)，
        // 該 serializer 反序列化後 DateTime.Kind = Unspecified，導致 DateTimeNoZConverter 不轉時區。
        // 此處強制 SpecifyKind 為 Utc，讓 converter 走 ConvertTimeFromUtc → Asia/Taipei 路徑。
        private static DateTime? AsUtc(DateTime? value)
            => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

        internal static SearchOrderInfoDataModel[] ConvertToOrderData(this IEnumerable<MongoOrder> mongoOrders)
        {
            return mongoOrders
                .Where(mo => mo is not null)
                .Select(MapMongoOrder)
                .Where(d => d is not null)
                .Select(d => d!)
                .ToArray();
        }

        private static SearchOrderInfoDataModel? MapMongoOrder(MongoOrder mo)
        {
            return new SearchOrderInfoDataModel
            {
                COrderM = MapOrderMaster(mo.COrderM, mo.CoomNo),
                COrderC = MapOrderCart(mo.COrderC, mo.CoocNo),
                COrderD = MapOrderItems(mo.COrderD),
                CGoodsItem = MapGoodsItem(mo.CGoodsItem),
                EShipmentM = MapShipmentMaster(mo.EShipmentM),
                CQuestionM = MapQuestionMaster(mo.CQuestionM),
                CCancelM = MapCancelMaster(mo.CCancelM),
                EShipmentL = mo.EShipmentL,
                EShipmentS = mo.EShipmentS,
                ECCDHL = MapCCDHL(mo.ECCDHL),
                ECCCS = MapCCCS(mo.ECCCS),
                ERtnDHLApply = MapRtnDHLApply(mo.ERtnDHLApply),
            };
        }

        private static OrderMasterModel? MapOrderMaster(COrderM? m, string? coomNo)
        {
            if (m is null && string.IsNullOrEmpty(coomNo)) return null;
            m ??= new COrderM();
            return new OrderMasterModel
            {
                CoomNo = coomNo,
                CoomOrderDate = AsUtc(m.CoomOrderDate),
                CoomName = m.CoomName,
                CoomStatus = m.CoomStatus,
                CoomTempType = m.CoomTempType,
                CoomCreateDatetime = AsUtc(m.CoomCreateDatetime),
                CoomCuamCid = m.CoomCuamCid,
                CoomReChoiceFlag = m.CoomRechoiceFlag,
                CoomMergeListCoomNo = m.CoomMergeListCoomNo,
                CoomSellerMemo = m.CoomSellerMemo,
                CoomSellerGoodsTotalAmt = m.CoomSellerGoodsTotalAmt,
                CoomGoodsItemNum = m.CoomGoodsItemNum,
                CoomGoodsTotalNum = m.CoomGoodsTotalNum,
                CoomRcvTotalAmt = m.CoomRcvTotalAmt,
                CoomCgdmId = m.CoomCgdmId,
                CoomShipPrintFlag = m.CoomShipPrintFlag,
                CoomCccmNo = m.CoomCccmNo,
            };
        }

        private static OrderCartModel? MapOrderCart(COrderC? c, string? coocNo)
        {
            if (c is null && string.IsNullOrEmpty(coocNo)) return null;
            c ??= new COrderC();
            return new OrderCartModel
            {
                CoocNo = coocNo,
                CoocPaymentType = c.CoocPaymentType,
                CoocPaymentPayDatetime = AsUtc(c.CoocPaymentPayDatetime),
                CoocDeliverMethod = c.CoocDeliverMethod,
                CoocOrdChannelKind = c.CoocOrdChannelKind,
                CoocCreateDatetime = AsUtc(c.CoocCreateDatetime),
                CoocMemSid = c.CoocMemSid,
                CoocPaymentCode = c.CoocPaymentCode,
                CoocOrdNameEnc = c.CoocOrdNameEnc,
                CoocRcvNameEnc = c.CoocRcvNameEnc,
                CoocRcvMobileEnc = c.CoocRcvMobileEnc,
                CoocPaymentTradeNo = c.CoocPaymentTradeNo,
                CoocPaymentNote = c.CoocPaymentNote,
                CoocPaymentBankCode = c.CoocPaymentBankCode,
                CoocPaymentDueday = AsUtc(c.CoocPaymentDueday),
            };
        }

        private static OrderItemModel[]? MapOrderItems(List<COrderD>? items)
        {
            if (items is null || items.Count == 0) return null;
            return items.Select(d => new OrderItemModel
            {
                CoodName = d.CoodName,
                CoodQty = d.CoodQty,
                CoodOriginalPrice = d.CoodPrice,
                CoodDiscountPrice = d.CoodDiscountPrice,
                CoodReceivePrice = d.CoodReceivePrice,
                CoodImagePath = d.CoodImagePath,
            }).ToArray();
        }

        // 對齊 Golden Recipe Search_2/3 樣張:即使 Mongo 端 nested 為 null,Public 端輸出殼結構(欄位 null)
        // 例外 e_Shipment_L / e_Shipment_S 樣張為 null(非殼),由 MapMongoOrder 直接 null pass-through

        private static GoodsItemModel MapGoodsItem(CGoodsItem? g) => new()
        {
            CgdiNumberColumn1 = g?.CgdiNumberColumn1,
        };

        private static ShipmentMasterModel MapShipmentMaster(EShipmentM? s) => new()
        {
            EsmmNo = s?.EsmmNo,
            EsmmShipNo = s?.EsmmShipNo,
            EsmmStatus = s?.EsmmStatus,
            EsmmShipMethod = s?.EsmmShipMethod,
            EsmmShipNoAuthCode = s?.EsmmShipNoAuthCode,
            EsmmShipNoA = s?.EsmmShipNoA,
            EsmmLeaveStoreDateB = AsUtc(s?.EsmmLeaveStoreDateB),
            EsmmIbonAppFlag = s?.EsmmIbonAppFlag,
            EsmmOddReason = s?.EsmmOddReason,
            EsmmConfirmExtpayDatetime = AsUtc(s?.EsmmConfirmExtpayDatetime),
        };

        // c_Question_M 殼預設 0(對齊 Golden 樣張「無問答」用 0 表示,非 null)
        private static QuestionMasterModel MapQuestionMaster(CQuestionM? q) => new()
        {
            SellerQaNeverReplyCount = q?.SellerQaNeverReplyCount ?? 0,
            BuyerQaNeverReplyCount = q?.BuyerQaNeverReplyCount ?? 0,
        };

        private static CancelMasterModel MapCancelMaster(CCancelM? c) => new()
        {
            CccmStatus = c?.CccmStatus,
            CccmCancelPeople = c?.CccmCancelPeople,
            CccmConfirmDatetime = AsUtc(c?.CccmConfirmDatetime),
            CccmCreateDatetime = AsUtc(c?.CccmCreateDatetime),
            CccmRefundFlag = c?.CccmRefundFlag,
            CccmErfmNo = c?.CccmErfmNo,
        };

        private static CCDHLModel MapCCDHL(ECCDHL? d) => new()
        {
            EcdhEsmmNo = d?.EcdhEsmmNo,
            EcdhProcessCode = d?.EcdhProcessCode,
        };

        private static CCCSModel MapCCCS(ECCCS? c) => new()
        {
            EccsId = c?.EccsId,
            EccsStoreType = c?.EccsStoreType,
            EccsRechoiceStoreStatus = c?.EccsRechoiceStoreStatus,
            EccsCreateDatetime = AsUtc(c?.EccsCreateDatetime),
        };

        private static RtnDHLApplyModel MapRtnDHLApply(ERtnDHLApply? r) => new()
        {
            ErdaApplyStatus = r?.ErdaApplyStatus,
        };
    }
}
