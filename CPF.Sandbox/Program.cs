using CPF.Sandbox.IntegrationTests.DataFactory;
using CPF.Sandbox.IntegrationTests.PipelineSeeders;
using CPF.Sandbox.IntegrationTests.Scenarios;
using CPF.Sandbox.Scenarios;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serialization;

// =============================================================================
//  CPF.Sandbox 開發測試入口
//  - 有 args(CLI / launchSettings profile)→ 直接跑指定 mode
//  - 無 args(VS F5 dev)→ 進入互動式選單,可重複切換不重啟
// =============================================================================

// DBSDK Part I §A:兩階段靜態初始化 (Mongo Serializer + ClassMap)
MongoSerializationConfig.Register();
MongoMap.EnsureClassMapsRegistered();

if (args.Length > 0)
{
    await DispatchAsync(args);
}
else
{
    await InteractiveMenuAsync();
}

PauseIfInteractive();


// =============================================================================
//  CMD 模式分派 (給 PM 抽驗 + launchSettings profile)
//  用法:dotnet run --project CPF.Sandbox -- <mode>
// =============================================================================
static async Task DispatchAsync(string[] args)
{
    var mode = args[0].ToLower();
    switch (mode)
    {
        // 資料植入
        case "seed":        await ElasticDataSeedScenario.RunAsync(); break;
        case "seed-golden": await GoldenSeeder.SeedAsync(); break;

        // Sandbox Suite 驗證 (跑全 Search 1~7 含 Check expected)
        case "validate":    await RunAllSearchSuiteAsync(); break;

        // 整合測試 (deterministic dataset 100 筆 cross orders-602/603/604)
        case "inttest":     await RunIntegrationTestAsync(args); break;

        // 偵錯出口 — 對比 Golden Recipe 用,verbose 輸出整 JSON
        case "dump-s1":     await P2_SearchScenarioSuite.RunSearch1Async(verbose: true); break;
        case "dump-s2":     await P2_SearchScenarioSuite.RunSearch2Async(verbose: true); break;
        case "dump-s3":     await P2_SearchScenarioSuite.RunSearch3Async(verbose: true); break;
        case "dump-s4":     await P2_SearchScenarioSuite.RunSearch4Async(verbose: true); break;
        case "dump-s5":     await P2_SearchScenarioSuite.RunSearch5Async(verbose: true); break;
        case "dump-s6":     await P2_SearchScenarioSuite.RunSearch6Async(verbose: true); break;
        case "dump-s7":     await P2_SearchScenarioSuite.RunSearch7Async(verbose: true); break;

        // SDK QuickStart 教學 (Sandbox 場景配合 docs/SDK_QuickStart.md)
        case "teaching": await IntegrationGuideScenario.RunAsync(); break;

        // 離線驗證 (Mock + Shipping + Update tests)
        case "offline":
        default:
            RunOfflineValidation();
            break;
    }
}


// =============================================================================
//  互動式選單 (給 VS F5 dev,跑完回選單可繼續切換)
// =============================================================================
static async Task InteractiveMenuAsync()
{
    while (true)
    {
        PrintMenu();
        Console.Write("請選擇: ");
        var key = Console.ReadKey(intercept: true);
        Console.WriteLine(key.KeyChar);
        Console.WriteLine();

        var choice = char.ToLower(key.KeyChar);
        if (choice == 'q' || key.Key == ConsoleKey.Escape) return;

        try
        {
            await RunChoiceAsync(choice);
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"❌ 執行錯誤: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine();
        Console.WriteLine("───────────────────────────────────────");
        Console.WriteLine("按任意鍵回選單;Q / Esc 結束...");
        var follow = Console.ReadKey(intercept: true);
        if (char.ToLower(follow.KeyChar) == 'q' || follow.Key == ConsoleKey.Escape) return;
    }
}

static void PrintMenu()
{
    Console.Clear();
    Console.WriteLine("╔══════════════════════════════════════════════════════╗");
    Console.WriteLine("║         CPF.Sandbox 開發測試選單                     ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════╝");
    Console.WriteLine();
    Console.WriteLine(" 【偵錯出口】(verbose JSON,對比 Golden Recipe)");
    Console.WriteLine("   1  Search 1 — GetHomeToDoOverview");
    Console.WriteLine("   2  Search 2 — SearchBySellerId       (Dual Engine)");
    Console.WriteLine("   3  Search 3 — SearchByBuyerId        (Dual Engine)");
    Console.WriteLine("   4  Search 4 — GetAppDashboardOverview");
    Console.WriteLine("   5  Search 5 — GetAppSalesToday");
    Console.WriteLine("   6  Search 6 — GetAppSalesWeek");
    Console.WriteLine("   7  Search 7 — GetUserCgdmData");
    Console.WriteLine("   A  跑全部 Search 1~7 (Suite + Check expected)");
    Console.WriteLine();
    Console.WriteLine(" 【資料植入】");
    Console.WriteLine("   S  ElasticDataSeedScenario");
    Console.WriteLine("   G  GoldenSeeder (灌客戶 Sample Data 進 ES + Mongo)");
    Console.WriteLine();
    Console.WriteLine(" 【整合測試】(100 筆 deterministic dataset)");
    Console.WriteLine("   I  E2E_S1~S7 (跑驗證)");
    Console.WriteLine("   J  ElasticSeeder (灌測試 dataset)");
    Console.WriteLine();
    Console.WriteLine(" 【離線驗證】(無需 ES/Mongo)");
    Console.WriteLine("   O  Mock + Shipping + S22ElasticUpdate");
    Console.WriteLine();
    Console.WriteLine(" 【SDK 整合教學】");
    Console.WriteLine("   T  QuickStart 教學 (配合 docs/SDK_QuickStart.md)");
    Console.WriteLine();
    Console.WriteLine("   Q / Esc  結束");
    Console.WriteLine();
}

static async Task RunChoiceAsync(char choice)
{
    switch (choice)
    {
        case '1': await P2_SearchScenarioSuite.RunSearch1Async(verbose: true); break;
        case '2': await P2_SearchScenarioSuite.RunSearch2Async(verbose: true); break;
        case '3': await P2_SearchScenarioSuite.RunSearch3Async(verbose: true); break;
        case '4': await P2_SearchScenarioSuite.RunSearch4Async(verbose: true); break;
        case '5': await P2_SearchScenarioSuite.RunSearch5Async(verbose: true); break;
        case '6': await P2_SearchScenarioSuite.RunSearch6Async(verbose: true); break;
        case '7': await P2_SearchScenarioSuite.RunSearch7Async(verbose: true); break;
        case 'a': await RunAllSearchSuiteAsync(); break;

        case 's': await ElasticDataSeedScenario.RunAsync(); break;
        case 'g': await GoldenSeeder.SeedAsync(); break;

        case 'i': await RunIntegrationTestAsync(new[] { "inttest", "validate" }); break;
        case 'j': await RunIntegrationTestAsync(new[] { "inttest", "seed" }); break;

        case 'o': RunOfflineValidation(); break;

        case 't': await IntegrationGuideScenario.RunAsync(); break;

        default:
            Console.WriteLine("(無效選項)");
            break;
    }
}


// =============================================================================
//  共用流程 (CMD 模式 + 互動模式共用)
// =============================================================================
static async Task RunAllSearchSuiteAsync()
{
    await P2_SearchScenarioSuite.RunSearch1Async();
    await P2_SearchScenarioSuite.RunSearch2Async();
    await P2_SearchScenarioSuite.RunSearch3Async();
    await P2_SearchScenarioSuite.RunSearch4Async();
    await P2_SearchScenarioSuite.RunSearch5Async();
    await P2_SearchScenarioSuite.RunSearch6Async();
    await P2_SearchScenarioSuite.RunSearch7Async();
}

static async Task RunIntegrationTestAsync(string[] args)
{
    var sub = args.Length > 1 ? args[1].ToLower() : "validate";
    var intDataset = OrderTestDataFactory.Build(seed: 42);

    if (sub == "seed")
    {
        await ElasticSeeder.SeedAsync(intDataset);
        return;
    }

    await E2E_S1_HomeOverview.RunAsync(intDataset);
    await E2E_S2_SearchBySeller.RunAsync(intDataset);
    await E2E_S3_SearchByBuyer.RunAsync(intDataset);
    await E2E_S4_AppDashboard.RunAsync(intDataset);
    await E2E_S5_AppSalesToday.RunAsync(intDataset);
    await E2E_S6_AppSalesWeek.RunAsync(intDataset);
    await E2E_S7_UserCgdmData.RunAsync(intDataset);
}

static void RunOfflineValidation()
{
    MockValidationScenario.Run();
    ShippingSyncScenario.Run();
    S22ElasticUpdateTest.RunAllTests();
}

static void PauseIfInteractive()
{
    if (!Console.IsInputRedirected && Environment.UserInteractive)
    {
        Console.WriteLine();
        Console.WriteLine("按任意鍵結束...");
        Console.ReadKey();
    }
}
