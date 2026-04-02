using NO3._dbSDK_Imporve.Core.Interface;

namespace NO3._dbSDK_Imporve.Application
{
    public class dbSDKEngine<T> : IEngine<T>
    {
        private readonly IRepository<T> _repository;
        public dbSDKEngine(IRepository<T> repository) { _repository = repository; }

        public async Task<IResult> Insert(T Data)
        {
            return await _repository.insertData(Data);
        }

        public async Task<IResult> Read(string ConditionData_Json)
        {
            return await _repository.getData(ConditionData_Json);
        }

        public async Task<IResult> Remove(string ConditionData_Json)
        {
            return await _repository.removeData(ConditionData_Json);
        }

        public async Task<IResult> Update(string ConditionData_Json, T Data)
        {
            return await _repository.updateData(ConditionData_Json, Data);
        }
    }
}
