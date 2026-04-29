using CPF.Sandbox.Scenarios;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serialization;
// 初始化 MongoDB 序列化器（確保 BsonElement 映射正確）
MongoSerializationConfig.Register();
MongoMap.EnsureClassMapsRegistered();

// S22：ElasticSearch 更新失效修復測試
S22ElasticUpdateTest.RunAllTests();

// S21：ElasticSearch JSON 完整測試
//S21ElasticJsonTest.RunAllTests();

// S12：全域沙盒場景 (包含 S17 貨態更新)
//SandboxRunner.RunAll();

// S13：步進驗證 — Insert → Read(V1) → Update(含null混入) → Read(V2) → 對比報告
//StatefulComparisonScenario.RunStepByStepVerification();

// S14：賣家取號模擬 (Status 20) — 物流模組掛載驗證
//SellerGetNumberScenario.RunSellerGetNumberSimulation();

// S15：寄貨完成模擬 (Status 30) — $set + $push 並行驗證
//ShippingCompleteScenario.RunShippingCompleteSimulation();

// S16：Elastic Search 強型別與 Mock 驗證
//ElasticSearchScenario.RunElasticSearchSimulation();

Console.WriteLine();
Console.WriteLine("按任意鍵結束...");
Console.ReadKey();
