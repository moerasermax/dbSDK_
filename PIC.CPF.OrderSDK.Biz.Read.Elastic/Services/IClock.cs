namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Services
{
    /// <summary>
    /// 時鐘抽象 — Application/BLL 層依此介面取得 "today"、
    /// 避免直接呼叫 DateTime.Now、利於測試注入固定時間。
    /// 對齊客戶原 SDK BLL 使用 DateTime.Now 取本地時間的語義 (Kind=Local)。
    /// </summary>
    public interface IClock
    {
        /// <summary>本地時間 (Kind=Local、客戶原邏輯 DateTime.Now)</summary>
        DateTime Now { get; }
    }

    /// <summary>
    /// 預設實作:System DateTime.Now、生產環境用。
    /// </summary>
    public sealed class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;
    }
}
