using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;


namespace NO3._dbSDK_Imporve.Application.Sample.Elastic
{
    public interface IOrderRepository_Elastic : IRepository<OrderSummary>
    {
        IResult changeTable(string tableName);
    }   
}
