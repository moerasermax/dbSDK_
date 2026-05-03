using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace CPF.Sandbox.Scenarios
{
    public static class S29_GetUserCgdmDataScenario
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== S29: Search 7 — GetUserCgdmData ===");
            Console.WriteLine("========================================");

            var svc = SearchSdkSetup.Build();

            // GoldenRecipe In
            var req = new OrderSearchRequest { CuamCid = 528672 };
            Console.WriteLine($"  In: cuamCid={req.CuamCid}");

            var result = await svc.GetUserCgdmDataAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var data  = result.Data!;
            var cgdm0 = data.Cgdm?.Length > 0 ? data.Cgdm[0] : null;
            var cgdm1 = data.Cgdm?.Length > 1 ? data.Cgdm[1] : null;

            Console.WriteLine($"  Out — cuamCid={data.CuamCid}, cgdm count={data.Cgdm?.Length}");
            if (cgdm0 != null) Console.WriteLine($"  Out — cgdm[0]: {cgdm0.CgdmId} @ '{cgdm0.CgdmUpdateDatetime}'");
            if (cgdm1 != null) Console.WriteLine($"  Out — cgdm[1]: {cgdm1.CgdmId} @ '{cgdm1.CgdmUpdateDatetime}'");

            Check("CuamCid",                    data.CuamCid,                   528672);
            Check("Cgdm count",                 data.Cgdm?.Length,              2);
            Check("Cgdm[0].CgdmId",             cgdm0?.CgdmId,                  "GM2508260014245");
            Check("Cgdm[1].CgdmId",             cgdm1?.CgdmId,                  "GM2512180014259");

            // CgdmUpdateDatetime 來自 _ord_modify_date 欄位的 max 聚合
            if (string.IsNullOrEmpty(cgdm0?.CgdmUpdateDatetime) && string.IsNullOrEmpty(cgdm1?.CgdmUpdateDatetime))
            {
                Console.WriteLine("  ⚠️ NO DATA: Cgdm[0/1].CgdmUpdateDatetime 皆為空字串。");
                Console.WriteLine("     原因：客戶範例資料 30 筆均無 \"_ord_modify_date\" 欄位，max 聚合無值可回傳。");
                Console.WriteLine("     GoldenRecipe Search_7 期望 \"2026-04-28T14:35:51.775\" / \"2026-04-28T14:35:36.628\"，需請客戶補 _ord_modify_date 才能驗證。");
            }
            else
            {
                Check("Cgdm[0].CgdmUpdateDatetime", cgdm0?.CgdmUpdateDatetime,      "2026-04-28T14:35:51.775");
                Check("Cgdm[1].CgdmUpdateDatetime", cgdm1?.CgdmUpdateDatetime,      "2026-04-28T14:35:36.628");
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
