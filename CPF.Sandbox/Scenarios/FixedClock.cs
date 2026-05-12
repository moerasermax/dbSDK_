using PIC.CPF.OrderSDK.Biz.Read.Elastic.Services;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// 測試專用 IClock 實作:回固定時間。
    /// 用途:Search 4 BLL fallback (mondayDate / endDate / today.AddDays(-90)) 計算需要 today、
    ///       測試環境注入固定時間 (2026-05-05 對齊 Golden Recipe 樣張生成時點)、
    ///       讓 Suite In 可只傳 {cuamCid}、由 BLL fallback 算出 Performance / Overview 區間
    ///       涵蓋測試資料 (全在 2026-05-05)、Out 對齊 Golden。
    /// </summary>
    internal sealed class FixedClock : IClock
    {
        private readonly DateTime _fixedTime;
        public FixedClock(DateTime fixedTime) { _fixedTime = fixedTime; }
        public DateTime Now => _fixedTime;
    }
}
