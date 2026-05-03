using CPF.Sandbox.IntegrationTests.DataFactory;
using CPF.Sandbox.IntegrationTests.PipelineSeeders;
using CPF.Sandbox.IntegrationTests.Scenarios;
using CPF.Sandbox.Scenarios;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serialization;

MongoSerializationConfig.Register();
MongoMap.EnsureClassMapsRegistered();

var mode = args.Length > 0 ? args[0].ToLower() : "offline";

switch (mode)
{
    // ── dotnet run --project CPF.Sandbox seed ─────────────────
    case "seed":
        await ElasticDataSeedScenario.RunAsync();
        break;

    // ── dotnet run --project CPF.Sandbox validate ─────────────
    case "validate":
        await S23_GetHomeToDoOverViewScenario.RunAsync();
        await S24_SearchBySellerScenario.RunAsync();
        await S25_SearchByBuyerScenario.RunAsync();
        await S26_GetAppDashboardScenario.RunAsync();
        await S27_GetAppSalesTodayScenario.RunAsync();
        await S28_GetAppSalesWeekScenario.RunAsync();
        await S29_GetUserCgdmDataScenario.RunAsync();
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
