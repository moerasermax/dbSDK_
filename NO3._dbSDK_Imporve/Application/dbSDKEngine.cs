using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using System.Text.Json;
using System.Xml.Linq;

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
        
        public async Task TestFlow(string ConditionData_Json, T Data)
        {

            EventGiftModel datajson = Data as EventGiftModel;
            datajson.Id += "Dev";
            string NewData = JsonSerializer.Serialize(datajson);


            await Insert(Data);
            Console.WriteLine($"已完成資料新增。請按一下繼續下一步......{datajson.event_id}"); Console.ReadKey();
            await Update(ConditionData_Json, (dynamic)datajson);
            Console.WriteLine($"已完成資料更新。請按一下繼續下一步......條件{ConditionData_Json} 更新為{NewData}"); Console.ReadKey();
            EventGiftModel result = await Read(ConditionData_Json) as EventGiftModel;
            Console.WriteLine($"已完成資料查詢。請按一下繼續下一步......{result.event_id}"); Console.ReadKey();
            await Remove(ConditionData_Json);
            Console.WriteLine($"已完成資料移除。請按一下結束測試流程......已刪除 {ConditionData_Json}資料"); Console.ReadKey();
        }


    }
}
