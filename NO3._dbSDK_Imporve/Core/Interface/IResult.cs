namespace NO3._dbSDK_Imporve.Core.Interface
{
    /// <summary>
    /// 基礎結果介面 (非泛型版本，保持向下相容)
    /// </summary>
    public interface IResult
    {
        bool IsSuccess { get; }
        string Msg { get; }
        string DataJson { get; }
    }

    /// <summary>
    /// 泛型結果介面 (型別安全版本)
    /// </summary>
    /// <typeparam name="T">資料型別</typeparam>
    public interface IResult<out T> : IResult
    {
        /// <summary>
        /// 強型別資料物件
        /// </summary>
        T? Data { get; }
    }
}
