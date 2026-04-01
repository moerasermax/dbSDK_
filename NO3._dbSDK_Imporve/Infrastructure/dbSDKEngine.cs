using Elastic.Clients.Elasticsearch.Snapshot;
using NO3._dbSDK_Imporve.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Infrastructure
{
    public class dbSDKEngine<T> : IEngine<T>
    {
        private readonly NO3._dbSDK_Imporve.Core.Interface.IRepository<T> _repository;
        public dbSDKEngine(NO3._dbSDK_Imporve.Core.Interface.IRepository<T> repository) { _repository = repository; }

        public async Task Insert(T Data)
        {

            await _repository.insertData(Data);
        }

        public async Task Read(string conditionData)
        {
            await _repository.getData(conditionData);
        }

        public async Task Remove(string conditionData)
        {
            await _repository.removeData(conditionData);
        }

        public async Task Update(string conditionData, T Data)
        {
            await _repository.updateData(conditionData, Data);
        }

        
    }
}
