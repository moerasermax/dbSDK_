using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;

namespace NO3._dbSDK_Imporve.Application.Sample.Redis
{
    public interface IOrderRepository_Redis : IRepository<Query>
    {
        public Task<IResult> pollingData();
    }


}
