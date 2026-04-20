using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Models;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Interfaces
{
    public interface IMongoDBRepository<T> : IRepository<T>
    {
        Task<IResult> UpdateInit(string ConditionData_Json, T UpdateData, MongoUpdateOptions options);
        Task<IResult> UpdateData(string ConditionData_Json, T UpdateData, MongoUpdateOptions options);
    }
}
