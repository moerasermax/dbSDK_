using CPF.Sandbox.IntegrationTests.DataFactory;
using CPF.Sandbox.IntegrationTests.PipelineSeeders;
using CPF.Sandbox.IntegrationTests.Scenarios;
using CPF.Sandbox.Scenarios;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serialization;
using System.Security.Cryptography.X509Certificates;

MongoSerializationConfig.Register();
MongoMap.EnsureClassMapsRegistered();

var mode = args.Length > 0 ? args[0].ToLower() : "offline";
switch (mode)
{
    // ── dotnet run --project CPF.Sandbox seed ─────────────────
    case "seed":
        await ElasticDataSeedScenario.RunAsync();
        break;

    // ── dotnet run --project CPF.Sandbox seed-golden ───────────
    // [S37] Golden Data 數據導入：植入客戶正式測試資料
    case "seed-golden":
        await GoldenSeeder.SeedAsync();
        break;

    // ── dotnet run --project CPF.Sandbox validate ─────────────
    case "validate":
        await P2_SearchScenarioSuite.RunSearch1Async();
        await P2_SearchScenarioSuite.RunSearch2Async();
        await P2_SearchScenarioSuite.RunSearch3Async();
        await P2_SearchScenarioSuite.RunSearch4Async();
        await P2_SearchScenarioSuite.RunSearch5Async();
        await P2_SearchScenarioSuite.RunSearch6Async();
        await P2_SearchScenarioSuite.RunSearch7Async();
        break;

    // ── dotnet run --project CPF.Sandbox inttest seed/validate ─
    // 整合測試：100 筆 deterministic dataset，跨 orders-602/603/604 三個 monthly index
    case "inttest":
        var sub = args.Length > 1 ? args[1].ToLower() : "validate";
        var intDataset = OrderTestDataFactory.Build(seed: 42);
        if (sub == "seed")
        {
            await ElasticSeeder.SeedAsync(intDataset);
        }
        else
        {
            await E2E_S1_HomeOverview.RunAsync(intDataset);
            await E2E_S2_SearchBySeller.RunAsync(intDataset);
            await E2E_S3_SearchByBuyer.RunAsync(intDataset);
            await E2E_S4_AppDashboard.RunAsync(intDataset);
            await E2E_S5_AppSalesToday.RunAsync(intDataset);
            await E2E_S6_AppSalesWeek.RunAsync(intDataset);
            await E2E_S7_UserCgdmData.RunAsync(intDataset);
        }
        break;

    // ── dotnet run --project CPF.Sandbox dump-s1 ~ dump-s7 ───
    // 偵錯入口：跑單一 Search method 並輸出 JSON, 用以對比客戶 GoldenRecipe Out
    case "dump-s1":
        await P2_SearchScenarioSuite.RunSearch1Async(verbose: true);
        break;
    case "dump-s2":
        await P2_SearchScenarioSuite.RunSearch2Async(verbose: true);
        break;
    case "dump-s3":
        await P2_SearchScenarioSuite.RunSearch3Async(verbose: true);
        break;
    case "dump-s4":
        await P2_SearchScenarioSuite.RunSearch4Async();
        break;
    case "dump-s5":
        await P2_SearchScenarioSuite.RunSearch5Async();
        break;
    case "dump-s6":
        await P2_SearchScenarioSuite.RunSearch6Async();
        break;
    case "dump-s7":
        await P2_SearchScenarioSuite.RunSearch7Async();
        break;

    // ── dotnet run --project CPF.Sandbox (預設離線驗證) ────────
    default:
        MockValidationScenario.Run();
        ShippingSyncScenario.Run();
        S22ElasticUpdateTest.RunAllTests();
        break;
}

Console.WriteLine();
Console.WriteLine("按任意鍵結束...");
Console.ReadKey();
