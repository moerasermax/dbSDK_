using CPF.Services.Redis.Post.Model;
using NO3._dbSDK_Imporve.Application.Sample.Redis;

namespace CPF.Services.Redis.Post
{
    public class Worker_ForCPF : BackgroundService
    {
        private readonly OrderRepository_Redis _redis;
        private readonly AddOrderEventRandomDataGenerator _CPF_TestDataEngine;

        public Worker_ForCPF(OrderRepository_Redis Redis
            , AddOrderEventRandomDataGenerator MongoDBCPF_TestDataEngine
            )
        {
            _redis = Redis;
            _CPF_TestDataEngine = MongoDBCPF_TestDataEngine;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)

            {
                System.Console.WriteLine("[Redis_Post]按下【1】，發送【一般】【新增訂單】的Request");
                System.Console.WriteLine("[Redis_Post]按下【2】，發送【一般】【訂單備註更新】的Request");
                System.Console.WriteLine("[Redis_Post]按下【3】，發送【一般】【變更付款方式】的Request");
                System.Console.WriteLine("[Redis_Post]按下【4】，發送【貨態】【取號】的Request (UpdateSellerGetNumberEvent)");
                System.Console.WriteLine("[Redis_Post]按下【5】，發送【貨態】【寄貨】的Request (Delivery_CargoDynamics_02)");

                var keyInfo = Console.ReadKey(intercept: true); // true 代表不把按下的字顯示在螢幕上

                if (keyInfo.KeyChar.ToString().Equals("1"))
                {
                    Createflow();
                }
                else if (keyInfo.KeyChar.ToString().Equals("2"))
                {
                    UpdateCoomSellerMemoFolw();
                }
                else if (keyInfo.KeyChar.ToString().Equals("3"))
                {
                    UpdateChangePayTypeEvent();
                }
                else if (keyInfo.KeyChar.ToString().Equals("4"))
                {
                    UpdateCargoDynamics01Flow();
                }
                else if (keyInfo.KeyChar.ToString().Equals("5"))
                {
                    UpdateCargoDynamics02Flow();
                }


                await Task.Delay(1000, stoppingToken);

            }
        }

        async void Createflow()
        {


            var TestData = _CPF_TestDataEngine.Generate();

            _redis.QueryDB = "Request_MongoDB";

            await _redis.InsertData(_CPF_TestDataEngine.GetMongoDataObject(TestData));


            /// Elastic

            _redis.QueryDB = "Request_Elastic";
            await _redis.InsertData(_CPF_TestDataEngine.GetElasticDataObject(TestData));

            Console.WriteLine($"[新增訂單 Request發送完成]");
        }

        async void UpdateCoomSellerMemoFolw()
        {
            var UpdateData = _CPF_TestDataEngine.GetMongoUpdateCoomSellerMemoObject("CM9600365368700");

            _redis.QueryDB = "Request_MongoDB";
            await _redis.InsertData(UpdateData);

            Console.WriteLine($"[更新備註資訊 Request發送完成]");
        }

        async void UpdateChangePayTypeEvent()
        {
            var UpdateData = _CPF_TestDataEngine.GetMongoUpdateChangePayTypeEventObject("CC3692967605991");

            _redis.QueryDB = "Request_MongoDB";
            await _redis.InsertData(UpdateData);

            Console.WriteLine($"[更新付款資訊 Request發送完成]");
        }

        public string GenerateRandomCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #region S18 貨態更新 Flow

        /// <summary>
        /// 取號事件 Flow (UpdateSellerGetNumberEvent)
        /// </summary>
        async void UpdateCargoDynamics01Flow()
        {
            // 先建立訂單，才能進行取號
            var TestData = _CPF_TestDataEngine.Generate();
            var coomNo = TestData.coom.CoomNo;
            var rcvTotalAmt = TestData.coom.CoomRcvTotalAmt ?? 138;

            // 1. 先新增訂單到 MongoDB
            _redis.QueryDB = "Request_MongoDB";
            await _redis.InsertData(_CPF_TestDataEngine.GetMongoDataObject(TestData));

            // 2. 新增訂單到 Elastic
            _redis.QueryDB = "Request_Elastic";
            await _redis.InsertData(_CPF_TestDataEngine.GetElasticDataObject(TestData));

            // 3. 發送取號事件到 MongoDB
            _redis.QueryDB = "Request_MongoDB";
            await _redis.InsertData(_CPF_TestDataEngine.GetUpdateSellerGetNumberEventObject(coomNo));

            // 4. 發送取號事件到 Elastic
            _redis.QueryDB = "Request_Elastic";
            await _redis.InsertData(_CPF_TestDataEngine.GetElasticUpdateSellerGetNumberEventObject(coomNo, rcvTotalAmt));

            Console.WriteLine($"[取號 Request發送完成] CoomNo: {coomNo}");
        }

        /// <summary>
        /// 寄貨事件 Flow (Delivery_CargoDynamics_02)
        /// </summary>
        async void UpdateCargoDynamics02Flow()
        {
            // 先建立訂單並完成取號，才能進行寄貨
            var TestData = _CPF_TestDataEngine.Generate();
            var coomNo = TestData.coom.CoomNo;
            var rcvTotalAmt = TestData.coom.CoomRcvTotalAmt ?? 138;

            // 1. 先新增訂單到 MongoDB
            _redis.QueryDB = "Request_MongoDB";
            await _redis.InsertData(_CPF_TestDataEngine.GetMongoDataObject(TestData));

            // 2. 新增訂單到 Elastic
            _redis.QueryDB = "Request_Elastic";
            await _redis.InsertData(_CPF_TestDataEngine.GetElasticDataObject(TestData));

            // 3. 發送取號事件到 MongoDB
            _redis.QueryDB = "Request_MongoDB";
            await _redis.InsertData(_CPF_TestDataEngine.GetUpdateSellerGetNumberEventObject(coomNo));

            // 4. 發送取號事件到 Elastic
            _redis.QueryDB = "Request_Elastic";
            await _redis.InsertData(_CPF_TestDataEngine.GetElasticUpdateSellerGetNumberEventObject(coomNo, rcvTotalAmt));

            // 5. 發送寄貨事件到 MongoDB
            _redis.QueryDB = "Request_MongoDB";
            await _redis.InsertData(_CPF_TestDataEngine.GetDeliveryCargoDynamics02Object(coomNo));

            // 6. 發送寄貨事件到 Elastic
            _redis.QueryDB = "Request_Elastic";
            await _redis.InsertData(_CPF_TestDataEngine.GetElasticDeliveryCargoDynamics02Object(coomNo, rcvTotalAmt));

            Console.WriteLine($"[寄貨 Request發送完成] CoomNo: {coomNo}");
        }

        #endregion
    }
}