using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.DTO;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Interfaces;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Models;


namespace NO3._dbSDK_Imporve.Application.Sample.Mongo
{
    /// <summary>
    /// 訂單 Repository (MongoDB 實作)
    /// 使用組合模式封裝底層 MongoRepository，符合 DIP 原則
    /// </summary>
    public class OrderRepository_Mongo : IRepository<Orders>, IMongoDBRepository<Orders>
    {
        private readonly IMongoDBRepository<Orders> _innerRepository;

        /// <summary>
        /// 建構子：組合底層 Repository，而非繼承
        /// </summary>
        public OrderRepository_Mongo(MongoDBDriver driver, MongoMap mongoMap, IDTO dto)
        {
            // 組合：內部建立 MongoRepository 實例
            _innerRepository = new MongoRepository<Orders>(driver, mongoMap, dto, "Order");
        }

        // ===== IRepository<T> 委派實作 =====

        public async Task<IResult> GetData(string ConditionData_Json)
            => await _innerRepository.GetData(ConditionData_Json);

        public async Task<IResult> InsertData(Orders Data)
            => await _innerRepository.InsertData(Data);

        public async Task<IResult> UpdateData(string ConditionData_Json, Orders Data)
            => await _innerRepository.UpdateData(ConditionData_Json, Data);

        public async Task<IResult> RemoveData(string ConditionData_Json)
            => await _innerRepository.RemoveData(ConditionData_Json);

        // ===== IMongoDBRepository<T> 特有方法委派 =====

        /// <summary>
        /// 進階更新：支援 $set 與 $unset 合併操作
        /// </summary>
        public async Task<IResult> UpdateData(string ConditionData_Json, Orders Data, MongoUpdateOptions options)
            => await _innerRepository.UpdateData(ConditionData_Json, Data, options);

        /// <summary>
        /// 初始化更新指令 (供進階使用者直接取得 BsonDocument)
        /// </summary>
        public async Task<IResult> UpdateInit(string ConditionData_Json, Orders Data, MongoUpdateOptions options)
            => await _innerRepository.UpdateInit(ConditionData_Json, Data, options);
    }
}
