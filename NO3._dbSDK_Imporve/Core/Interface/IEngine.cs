namespace NO3._dbSDK_Imporve.Core.Interface
{
    public interface IEngine<T>
    {
        Task<IResult> Insert(T Data);
        Task<IResult> Update(string ConditionData_Json, T Data);
        Task<IResult> Read(string ConditionData_Json);
        Task<IResult> Remove(string ConditionData_Json);
    }
}
