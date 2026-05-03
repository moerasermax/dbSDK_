using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace CPF.Sandbox.IntegrationTests.DataFactory
{
    /// <summary>
    /// 整合測試資料集：100 筆 OrderDocument + 元資料。
    /// 由 OrderTestDataFactory 產生，作為 Source of Truth 給 ExpectedValueCalculator。
    /// </summary>
    public class TestDataset
    {
        public required IReadOnlyList<OrderDocument> Documents { get; init; }
        public required DateTime DateRangeStart { get; init; }
        public required DateTime DateRangeEnd { get; init; }
        public required int SellerCuamCid { get; init; }
        public required int BuyerMemSid { get; init; }
        public required int Seed { get; init; }
    }
}
