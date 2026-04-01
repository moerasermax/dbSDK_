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

        public async Task<IResult> Read(string conditionData)
        {
            return await _repository.getData(conditionData);
        }

        public async Task<IResult> Remove(string conditionData)
        {
            return await _repository.removeData(conditionData);
        }

        public async Task<IResult> Update(string conditionData, T Data)
        {
            return await _repository.updateData(conditionData, Data);
        }


        
    }
}
