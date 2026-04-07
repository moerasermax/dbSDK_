namespace NO3._dbSDK_Imporve.Core.Interface
{
    public interface IRepository<T>
    {
        Task<IResult> InsertData(T Data);
        Task<IResult> RemoveData(string ConditionData_Json);
        Task<IResult> UpdateData(string ConditionData_Json, T UpdateData);
        Task<IResult> GetData(string ConditionData_Json);
    }
}
