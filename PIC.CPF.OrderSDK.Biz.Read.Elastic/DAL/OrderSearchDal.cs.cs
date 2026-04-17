using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL
{
    public partial class OrderSearchDal
    {
        /// <summary>
        /// 訂單狀態-待取件: 已送達
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> OrderStateToFinishQuery()
        {
            return q => q.Term(t => t.Field(f => f.EsmmStatus).Value("20"));
        }

        /// <summary>
        /// 訂單狀態-取消訂單: 取消訂單 OR 已合併 OR 已合併取消
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> OrderStateCancelQuery()
        {
            return q => q.Terms(t => t
                .Field(f => f.CoomStatus)
                .Terms(new TermsQueryField(new FieldValue[] { "11", "12", "1X" }))
            );
        }

        /// <summary>
        /// 有買家未回覆 (使用最新 .Number 語法)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> BuyerQaNeverReplyQuery()
        {
            // 將 .NumberRange 改為 .Number
            return q => q.Range(r => r
                .Number(n => n.Field(f => f.BuyerQaNeverReplyCount).Gt(0))
            );
        }
        public static Action<QueryDescriptor<OrderDocument>> SellerQaNeverReplyQuery()
        {
            // 將 .NumberRange 改為 .Number
            return q => q.Range(r => r
                .Number(n => n.Field(f => f.SellerQaNeverReplyCount).Gt(0))
            );
        }

        /// <summary>
        /// 訂單狀態-已取件: 已取件
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> OrderStateFinishQuery()
        {
            return q => q.Term(t => t.Field(f => f.EsmmStatus).Value("30"));
        }

        /// <summary>
        /// 訂單狀態-退貨申請: 退貨申請
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> OrderStateRtnShippingQuery()
        {
            return q => q.Term(t => t.Field(f => f.CrsaApplied).Value(true));
        }

        /// <summary>
        /// 訂單日期 - 開始 (順手把 DateRange 也修了)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> OrderDateStartQuery(DateTime orderDateStart)
        {
            // 同理，將 .DateRange 改為 .Date
            return q => q.Range(r => r
                .Date(d => d.Field(f => f.CoomCreateDatetime).Gte(orderDateStart))
            );
        }
        /// <summary>
        /// 依訂購人會員編號精準比對 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> CoocMemSidQuery(int coocMemSid, int[]? bindMembers)
        {
            // 1. 處理會員編號集合 (這部分純 C# 邏輯維持不變)
            var memSidList = new List<int> { coocMemSid };
            if (bindMembers != null && bindMembers.Length > 0)
            {
                memSidList.AddRange(bindMembers);
            }

            // 取得不重複的整數陣列
            var memSidArray = memSidList.Distinct().ToArray();

            // 2. 轉換為 v8 的 Terms 查詢
            // 注意：int[] 需要轉換為 FieldValue[]
            return q => q.Terms(t => t
                .Field(f => f.CoocMemSid)
                .Terms(new TermsQueryField(memSidArray.Select(id => (FieldValue)id).ToArray()))
            );
        }

        /// <summary>
        /// 依訂購單編號部分比對 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> CoomNoQuery(string coomNo)
        {
            if (coomNo.Length == 15)
            {
                return q => q.Term(t => t.Field(f => f.CoomNo).Value(coomNo));
            }

            return q => q.Wildcard(w => w.Field(f => f.CoomNo).Value($"*{coomNo}*"));
        }

        /// <summary>
        /// 依賣場名稱部分比對 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> CoomNameQuery(string coomName)
        {
            return q => q.Wildcard(w => w.Field(f => f.CoomName).Value($"*{coomName}*"));
        }

        /// <summary>
        /// 依配送單編號部分比對 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> EsmmShipNoQuery(string esmmShipNo)
        {
            return q => q.Wildcard(w => w.Field(f => f.EsmmShipNo).Value($"*{esmmShipNo}*"));
        }

        /// <summary>
        /// 依購物車編號部分比對 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> CoocNoQuery(string coocNo)
        {
            if (coocNo.Length == 15)
            {
                return q => q.Term(t => t.Field(f => f.CoocNo).Value(coocNo));
            }

            return q => q.Wildcard(w => w.Field(f => f.CoocNo).Value($"*{coocNo}*"));
        }

        /// <summary>
        /// 依訂購單建立時間執行日期範圍查詢 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> OrderDateEndQuery(DateTime orderDateEnd)
        {
            // 使用最新的 .Date 語法避開 Obsolete
            return q => q.Range(r => r
                .Date(d => d.Field(f => f.CoomCreateDatetime).Lte(orderDateEnd))
            );
        }

        /// <summary>
        /// 依購物車收單類型精準比對 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> OrdChannelKindSearchKindQuery(string ordChannelKindSearchKind)
        {
            return q => q.Term(t => t.Field(f => f.CoocOrdChannelKind).Value(ordChannelKindSearchKind));
        }

        /// <summary>
        /// 依運送方式精準比對 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> DeliverMethodSearchKindQuery(string deliverMethodSearchKind)
        {
            return q => q.Term(t => t.Field(f => f.CoocDeliverMethod).Value(deliverMethodSearchKind));
        }

        /// <summary>
        /// 依溫層代碼精準比對 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> TempTypeSearchKindQuery(string tempTypeSearchKind)
        {
            return q => q.Term(t => t.Field(f => f.CoomTempType).Value(tempTypeSearchKind));
        }

        /// <summary>
        /// 依商品名稱部分比對
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> CoodNameQuery(string coodName)
        {
            return q => q.Wildcard(w => w.Field(f => f.CoomName).Value($"*{coodName}*"));
        }
        /// <summary>
        /// 訂單狀態-異常訂單: 寄件異常 OR 宅配退件 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> OrderStateExceptionQuery()
        {
            // Terms 寫法：確保使用 TermsQueryField 包裝字串陣列
            return q => q.Terms(t => t
                .Field(f => f.EsmmStatus)
                .Terms(new TermsQueryField(new FieldValue[] { "11", "32" }))
            );
        }

        /// <summary>
        /// 訂單狀態-未取退宅: 逾期未取件 OR 宅配退貨 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> OrderStateNoShowToDHLQuery()
        {
            // Should 寫法：在 Bool 描述器中直接串接 Should 與 MinimumShouldMatch
            return q => q.Bool(b => b
                .Should(
                    s => s.Term(t => t.Field(f => f.EsmmStatus).Value("31")),
                    s => s.Term(t => t.Field(f => f.IsReturnShipping).Value(true))
                )
                .MinimumShouldMatch(1)
            );
        }

        /// <summary>
        /// 訂單狀態-處理中: 訂單成立 AND (取貨付款 OR 已付款) (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> OrderStateDealWithQuery()
        {
            return q => q.Bool(b => b.Must(
                m => m.Term(t => t.Field(f => f.CoomStatus).Value("10")),
                m => m.Bool(innerB => innerB.Should(
                    s => s.Exists(e => e.Field(f => f.CoocPaymentPayDatetime)),
                    s => s.Term(t => t.Field(f => f.CoocPaymentType).Value("1"))
                ).MinimumShouldMatch(1))
            ));
        }

        /// <summary>
        /// 訂單狀態-待出貨 (賣家): 待寄件 AND (取貨付款 OR 已付款) (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> OrderStateToshipForSellerQuery()
        {
            return q => q.Bool(b => b.Must(
                m => m.Term(t => t.Field(f => f.EsmmStatus).Value("01")),
                m => m.Bool(innerB => innerB.Should(
                    s => s.Exists(e => e.Field(f => f.CoocPaymentPayDatetime)),
                    s => s.Term(t => t.Field(f => f.CoocPaymentType).Value("1"))
                ).MinimumShouldMatch(1))
            ));
        }

        /// <summary>
        /// 訂單狀態-已寄件: 已寄件 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> OrderStateShippingQuery()
        {
            return q => q.Term(t => t.Field(f => f.EsmmStatus).Value("10"));
        }

        /// <summary>
        /// 依訂單成立時間執行日期範圍查詢 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> CoomCreateDatetimeQuery(DateTime coomDateStart, DateTime coomDateEnd)
        {
            // 使用 .Date() 語法避開 Obsolete 警告，並使用 .Gte() 與 .Lte()
            return q => q.Range(r => r
                .Date(d => d
                    .Field(f => f.CoomCreateDatetime)
                    .Gte(coomDateStart)
                    .Lte(coomDateEnd)
                )
            );
        }

        /// <summary>
        /// 購買訂單查詢：排除異常與取消狀態 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> PurchaseOrderQuery()
        {
            // MustNot 搭配 Terms
            return q => q.Bool(b => b.MustNot(mn => mn
                .Terms(t => t
                    .Field(f => f.CoomStatus)
                    .Terms(new TermsQueryField(new FieldValue[] { "00", "11", "12", "1X" }))
                )
            ));
        }
        /// <summary>
        /// 有已取件時間，配送狀態為 [30: 已取件] 的時間戳記 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> EsmlStatusFinishDateTimeQuery(DateTime esmmDateStart, DateTime esmmDateEnd)
        {
            // 使用 .Range() 搭配 .Date() 處理日期區間
            return q => q.Range(r => r
                .Date(d => d
                    .Field(f => f.EsmlStatusFinishDateTime)
                    .Gte(esmmDateStart)
                    .Lte(esmmDateEnd)
                )
            );
        }
        /// <summary>
        /// 訂單總覽-已寄件: 已寄件 (V8 版)
        /// 20241126 ACHU 討論認為只要寄件就算
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> SellerOverviewShippingQuery()
        {
            // 回傳型別改為 Action，邏輯維持 Term 查詢
            return q => q.Term(t => t.Field(f => f.CoomStatus).Value("30"));
        }

        /// <summary>
        /// 訂單狀態-賣家總覽待退貨 (V8 版): 
        /// (逾期未取件 AND 無賣家取件註記) OR (已寄件 AND 正在退回中) OR (宅配退回註記)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> SellerOverviewWaitReturnQuery()
        {
            return q => q.Bool(b => b
                .Should(
                    // 條件 1: 逾期未取件 (31) AND 尚未由賣家取件 (Does Not Exist)
                    s => s.Bool(sb => sb.Must(
                        m => m.Term(t => t.Field(f => f.EsmmStatus).Value("31")),
                        m => m.Bool(innerB => innerB.MustNot(
                            mn => mn.Exists(e => e.Field(f => f.EsmsDlvstatusSellerPickup))
                        ))
                    )),
                    // 條件 2: 已寄件 (10) AND 店到店正在退回註記 (True)
                    s => s.Bool(sb => sb.Must(
                        m => m.Term(t => t.Field(f => f.EsmmStatus).Value("10")),
                        m => m.Term(t => t.Field(f => f.EsmsSCStatusReturning).Value(true))
                    )),
                    // 條件 3: 宅配退回註記 (True)
                    s => s.Term(t => t.Field(f => f.IsReturnShipping).Value(true))
                )
                .MinimumShouldMatch(1) // 至少符合以上其中一項
            );
        }


        /// <summary>
        /// 有已寄件時間，配送狀態為 [10: 已寄件] 的時間戳記 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> EsmlStatusShippingDateTimeQuery(DateTime esmmDateStart, DateTime esmmDateEnd)
        {
            // 使用 .Range() 搭配 .Date() 處理出貨時間區間
            return q => q.Range(r => r
                .Date(d => d
                    .Field(f => f.EsmlStatusShippingDateTime)
                    .Gte(esmmDateStart) // GreaterThanOrEquals
                    .Lte(esmmDateEnd)    // LessThanOrEquals
                )
            );
        }
        /// <summary>
        /// 取貨倒數：區間內的訂單，並距離退貨剩3天之包裹數量 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> EsmlStatusWaitPickQuery()
        {
            // 計算今天日期加上3天的時間 (邏輯維持不變)
            DateTime thresholdDate = DateTime.Now.AddDays(3).Date;

            return q => q.Bool(b => b.Must(
                // 1. 配送狀態為 [20: 已送達]
                m => m.Term(t => t.Field(f => f.EsmmStatus).Value("20")),
                // 2. 包裹的離店日期小於等於閾值日期 (使用 v8 Date Range 寫法)
                m => m.Range(r => r
                    .Date(d => d
                        .Field(f => f.EsmmLeaveStoreDateB)
                        .Lte(thresholdDate)
                    )
                )
            ));
        }

        /// <summary>
        /// 查詢訂單狀態為 [10, 20, 30] 且已完成付款的訂單 (V8 版)
        /// </summary>
        public static Action<QueryDescriptor<OrderDocument>> CoomStatusSalesOrderQuery()
        {
            return q => q.Bool(b => b.Must(
                // 1. 訂單狀態：多值比對 (使用 TermsQueryField)
                m => m.Terms(t => t
                    .Field(f => f.CoomStatus)
                    .Terms(new TermsQueryField(new FieldValue[] { "10", "20", "30" }))
                ),
                // 2. 已完成付款條件：(有付款時間 OR 付款方式為取貨付款)
                m => m.Bool(innerB => innerB.Should(
                    s => s.Exists(e => e.Field(f => f.CoocPaymentPayDatetime)),  // 有付款時間
                    s => s.Term(t => t.Field(f => f.CoocPaymentType).Value("1")) // 付款方式為取貨付款
                ).MinimumShouldMatch(1))
            ));
        }
    }
}
