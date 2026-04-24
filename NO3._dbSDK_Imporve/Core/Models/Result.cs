using NO3._dbSDK_Imporve.Core.Interface;
using System.Text.Json;


namespace NO3._dbSDK_Imporve.Core.Models
{
    /// <summary>
    /// 基礎結果類別 (非泛型版本)
    /// </summary>
    public class Result : IResult
    {
        public bool IsSuccess { get; }
        public string Msg { get; }
        public string DataJson { get; }

        protected Result(bool isSuccess, string msg, string datajson)
        {
            IsSuccess = isSuccess;
            Msg = msg;
            DataJson = datajson;
        }

        // ===== 非泛型工廠方法 (保持向下相容) =====

        public static Result SetResult(string msg, bool isSuccess = true)
            => new(isSuccess, msg, "");

        public static Result SetResult(string msg, string DataJson, bool isSuccess = true)
            => new(isSuccess, msg, DataJson);

        public static Result SetErrorResult(string MethodName, string msg, bool isSuccess = false)
            => new(isSuccess, string.Format("發生錯誤請檢查，函示名稱{0}\r\n錯誤訊息：{1}", MethodName, msg), "");
    }

    /// <summary>
    /// 泛型結果類別 (型別安全版本)
    /// </summary>
    /// <typeparam name="T">資料型別</typeparam>
    public class Result<T> : Result, IResult<T>
    {
        /// <summary>
        /// 強型別資料物件
        /// </summary>
        public T? Data { get; }

        private Result(bool isSuccess, string msg, T? data, string dataJson)
            : base(isSuccess, msg, dataJson)
        {
            Data = data;
        }

        // ===== 泛型工廠方法 =====

        /// <summary>
        /// 建立成功結果 (帶強型別資料)
        /// </summary>
        public static Result<T> SetResult(string msg, T data, bool isSuccess = true)
        {
            var dataJson = data != null ? JsonSerializer.Serialize(data) : "";
            return new Result<T>(isSuccess, msg, data, dataJson);
        }

        /// <summary>
        /// 建立成功結果 (帶資料與自訂 JSON)
        /// </summary>
        public static Result<T> SetResult(string msg, T data, string dataJson, bool isSuccess = true)
        {
            return new Result<T>(isSuccess, msg, data, dataJson);
        }

        /// <summary>
        /// 建立錯誤結果
        /// </summary>
        public static new Result<T> SetErrorResult(string MethodName, string msg, bool isSuccess = false)
        {
            return new Result<T>(isSuccess, string.Format("發生錯誤請檢查，函示名稱{0}\r\n錯誤訊息：{1}", MethodName, msg), default, "");
        }

        /// <summary>
        /// 從非泛型 Result 轉換 (用於向下相容)
        /// </summary>
        public static Result<T> FromResult(Result result, T? data = default)
        {
            return new Result<T>(result.IsSuccess, result.Msg, data, result.DataJson);
        }
    }
}
