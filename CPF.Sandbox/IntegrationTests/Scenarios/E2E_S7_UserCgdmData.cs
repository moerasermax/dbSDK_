using CPF.Sandbox.IntegrationTests.DataFactory;
using CPF.Sandbox.Scenarios;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace CPF.Sandbox.IntegrationTests.Scenarios
{
    public static class E2E_S7_UserCgdmData
    {
        public static async Task RunAsync(TestDataset dataset)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== E2E S7: GetUserCgdmData ===");
            Console.WriteLine("========================================");

            var svc = SearchSdkSetup.Build();
            var req = new OrderSearchRequest { CuamCid = dataset.SellerCuamCid };
            var expected = ExpectedValueCalculator.Calculate_Search7(dataset, dataset.SellerCuamCid);

            var result = await svc.GetUserCgdmDataAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var data = result.Data!;
            Console.WriteLine($"  Out — CuamCid={data.CuamCid}, Cgdm count={data.Cgdm?.Length}");
            for (int i = 0; i < Math.Min(data.Cgdm?.Length ?? 0, 5); i++)
                Console.WriteLine($"  Out — cgdm[{i}]: {data.Cgdm![i].CgdmId} @ '{data.Cgdm![i].CgdmUpdateDatetime}'");
            Console.WriteLine($"  Expected: count={expected.Cgdm.Count}");
            for (int i = 0; i < Math.Min(expected.Cgdm.Count, 5); i++)
                Console.WriteLine($"  Expected — cgdm[{i}]: {expected.Cgdm[i].CgdmId} @ '{expected.Cgdm[i].CgdmUpdateDatetime}'");

            Check("CuamCid",    data.CuamCid,         expected.CuamCid);
            Check("Cgdm count", data.Cgdm?.Length,    expected.Cgdm.Count);

            // 比對前 N 個（ES doc_count tie 可能順序非 deterministic，只比 IDs set 與時間）
            var actualIds = (data.Cgdm ?? Array.Empty<CgdmDataModel>())
                .Select(c => c.CgdmId).OrderBy(s => s).ToList();
            var expectedIds = expected.Cgdm.Select(c => c.CgdmId).OrderBy(s => s).ToList();
            Check("Cgdm IDs (sorted)", string.Join(",", actualIds), string.Join(",", expectedIds));

            // 對每個 cgdmid 檢查 max date（順序無關）
            foreach (var (cgdmId, expDate) in expected.Cgdm)
            {
                var actualEntry = data.Cgdm?.FirstOrDefault(c => c.CgdmId == cgdmId);
                Check($"Cgdm[{cgdmId}].UpdateDatetime", actualEntry?.CgdmUpdateDatetime, expDate);
            }
            Console.WriteLine("========================================\n");
        }

        private static void Check<T>(string name, T? actual, T expected)
        {
            var pass = EqualityComparer<T>.Default.Equals(actual, expected);
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected={expected}, actual={actual}");
        }
    }
}
