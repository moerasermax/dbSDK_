using NO3._dbSDK_Imporve.Core.Models;


namespace NO3._dbSDK_Imporve.Core.Interface
{
    public interface IRepository<T>
    {
        Task<IResult> insertData(T Data);
        Task<IResult> removeData(string ConditionData);
        Task<IResult> updateData(string ConditionData, T UpdateData);
        Task<IResult> getData(string ConditionData);
    }
}
