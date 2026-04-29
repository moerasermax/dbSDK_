using NO3._dbSDK_Imporve.Core.Entity;
using System.Reflection;
using System.Text.Json;

namespace NO3._dbSDK_Imporve.Application.Sample.Elastic
{
    /// <summary>
    /// Elastic 指令建構工具
    /// 處理 Elastic 專用的資料對應與更新邏輯
    /// 
    /// 與 MongoDB 不同：
    /// - Mongo: $push 追加陣列
    /// - Elastic: 更新欄位（Partial Document Update）
    /// 
    /// S16：將 Elasticsearch 納入沙盒自動化檢核體系
    /// </summary>
    public static class ElasticCommandBuilder
    {
        /// <summary>
        /// 合併更新（Elastic Partial Document Update 邏輯）
        /// 將 updateDoc 的非 null 欄位合併到現有文件
        /// </summary>
        public static OrderSummary MergeUpdate(OrderSummary existing, OrderSummary updateDoc)
        {
            var result = CloneModel(existing);
            var updateType = updateDoc.GetType();

            foreach (var prop in updateType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = prop.GetValue(updateDoc);
                if (value != null)
                {
                    var resultProp = result.GetType().GetProperty(prop.Name);
                    resultProp?.SetValue(result, value);
                }
            }

            return result;
        }

        /// <summary>
        /// 產生 Status 30 的 Elastic 更新模型
        /// 對應 MongoDB 的 $push 行為，但 Elastic 是更新欄位
        /// </summary>
        public static OrderSummary CreateStatus30Update(string coomNo, DateTime shippingDateTime)
        {
            return new OrderSummary
            {
                PK = coomNo,
                CoomStatus = "30",
                EsmmStatus = "10",
                EsmlStatusShippingDatetime = shippingDateTime
            };
        }

        /// <summary>
        /// 產生 Status 20 的 Elastic 更新模型
        /// 對應 MongoDB 的物流模組掛載
        /// </summary>
        public static OrderSummary CreateStatus20Update(string coomNo, string esmmNo, string shipNo, string esmmStatus)
        {
            return new OrderSummary
            {
                PK = coomNo,
                CoomStatus = "20",
                EsmmShipNo = shipNo,
                EsmmStatus = esmmStatus
            };
        }

        /// <summary>
        /// 複製 OrderSummary（深拷貝）
        /// </summary>
        private static OrderSummary CloneModel(OrderSummary source)
        {
            var json = JsonSerializer.Serialize(source);
            return JsonSerializer.Deserialize<OrderSummary>(json) ?? new OrderSummary();
        }
    }
}