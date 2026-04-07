using NO3._dbSDK_Imporve.Core.Interface;

namespace NO3._dbSDK_Imporve.Application
{
    public class DbSDKEngine<T> : IEngine<T>
    {
        private readonly IRepository<T> _repository;
        public DbSDKEngine(IRepository<T> repository) { _repository = repository; }

        public async Task<IResult> Insert(T Data)
        {
            return await _repository.InsertData(Data);
        }

        public async Task<IResult> Read(string ConditionData_Json)
        {
            return await _repository.GetData(ConditionData_Json);
        }

        public async Task<IResult> Remove(string ConditionData_Json)
        {
            return await _repository.RemoveData(ConditionData_Json);
        }

        public async Task<IResult> Update(string ConditionData_Json, T Data)
        {
            return await _repository.UpdateData(ConditionData_Json, Data);
        }
    }
}
