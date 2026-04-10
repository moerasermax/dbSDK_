using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using StackExchange.Redis;
using System.Reflection;
using System.Text.Json;


namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Redis
{
    public abstract class RedisRepository<T> : IRepository<T> 
    {
        IDatabase _db;

        public RedisRepository(RedisDriver Driver, string TableName)
        {
            _db = Driver.GetDataBase();
        }
        public async Task<IResult> InsertData(T Data)
        {
            string RequestData = JsonSerializer.Serialize((object)Data, new JsonSerializerOptions() { IncludeFields = true });
            string respose = "";
            try
            {

                var response = await _db.ListRightPushAsync(GetKey(), RequestData);
                respose += string.Format("{0} 新增成功\r\n", RequestData);

                return Result.SetResult(string.Format("[Redis]資料新增成功。\r\n{0}", respose));
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }

        public async Task<IResult> GetData(string ConditionData_Json)
        {
            try
            {
                RedisValue result = await _db.ListLeftPopAsync(ConditionData_Json);
                return Result.SetResult("[RedisSDK]查詢資料成功。", result);
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }
        public async Task<IResult> RemoveData(string ConditionData_Json)
        {
            try
            {
                var response = await _db.KeyDeleteAsync(ConditionData_Json);
                return Result.SetResult("[RedisSDK]刪除資料成功。");
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }
        public async Task<IResult> PollingData()
        {
            try
            {
                var redisData = await _db.ListLeftPopAsync("Query");
                string queryResult = redisData.ToString();

                if (!queryResult.Contains("Result\":null"))
                {
                    return Result.SetResult("[RedisSDK]資料拉取成功。", queryResult);
                }
                else
                {
                    return Result.SetResult("[RedisSDK]目前Redis Buffer是空的");
                }
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }

        public async Task<IResult> UpdateData(string ConditionData_Json, T UpdateData)
        {
            return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, "[Redis]因Redis本身機制，再新增資料時若有相同的Key變會直接覆蓋，因此不需進行此實作。");
        }

        public abstract string GetKey();
    }
}
