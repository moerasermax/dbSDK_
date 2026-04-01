using MongoDB.Bson;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Redis
{
    public class RedisRepository<T> : IRepository<T>
    {
        IDatabase _db;

        public RedisRepository(RedisDriver Driver, string TableName)
        {
            _db = Driver.GetDataBase();
        }
        public async Task<IResult> insertData(T Data)
        {
            Dictionary<string, string> dic = RedisMap.GetObjectProperties<T>(Data);
            string respose = "";
            try
            {
                foreach (var item in dic)
                {
                    await _db.StringSetAsync(item.Key, item.Value);
                    respose += string.Format("{0} 新增成功\r\n", item.Value);
                }
                return Result.setResult(string.Format("[Redis]資料新增成功。\r\n{0}", respose));
            }
            catch (Exception ex)
            {
                return Result.setErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }

        public async Task<IResult> getData(string ConditionData)
        {
            try
            {
                var result = await _db.StringGetAsync(ConditionData);
                return Result.setResult("[RedisSDK]查詢資料成功。", result.ToJson());
            }
            catch (Exception ex)
            {
                return Result.setErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }
        public async Task<IResult> removeData(string ConditionData)
        {
            try
            {
                var response = await _db.KeyDeleteAsync(ConditionData);
                return Result.setResult("[RedisSDK]刪除資料成功。");
            }
            catch (Exception ex)
            {
                return Result.setErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }
        public async Task<IResult> pollingData()
        {
            try
            {
                var redisData = await _db.ListLeftPopAsync("Query");
                string queryResult = redisData.ToString();

                if (!queryResult.Contains("Result\":null"))
                {
                    return  Result.setResult("[RedisSDK]資料拉取成功。", queryResult);
                }
                else
                {
                    return  Result.setResult("[RedisSDK]目前Redis Buffer是空的");
                }
            }
            catch (Exception ex)
            {
                return Result.setErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }


        public Task<IResult> updateData(string ConditionData, T UpdateData)
        {
            throw new NotImplementedException("[Redis]因Redis本身機制，再新增資料時若有相同的Key變會直接覆蓋，因此不需進行此實作。");
        }
    }
}
