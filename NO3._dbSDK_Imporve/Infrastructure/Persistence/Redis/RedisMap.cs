using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Redis
{
    public class RedisMap
    {
        /// <summary>
        /// 將物件拆解為 屬性名稱 與 屬性值 的集合
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="data">物件資料</param>
        /// <returns>Dictionary (Key: 屬性名, Value: 內容字串)</returns>
        public static Dictionary<string, string> GetObjectProperties<T>(T data)
        {
            var result = new Dictionary<string, string>();

            if (data == null) return result;

            // 取得該類型的所有公開執行個體屬性 (Public Instance Properties)
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                // 取得屬性名稱
                string name = prop.Name;

                // 取得屬性值，並處理 Null 的情況
                object value = prop.GetValue(data);
                string stringValue = value?.ToString() ?? string.Empty;

                result.Add(name, stringValue);
            }

            return result;
        }
    }
}
