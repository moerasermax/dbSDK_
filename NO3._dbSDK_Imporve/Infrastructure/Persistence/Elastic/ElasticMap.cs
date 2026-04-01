using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic
{
    public class ElasticMap
    {
        public ElasticFilter toFilter(string ConditionData_Json)
        {
            ElasticFilter filter = new ElasticFilter();
            using (JsonDocument doc = JsonDocument.Parse(ConditionData_Json))
            {
                JsonElement root = doc.RootElement;

                // 遍歷所有的屬性
                foreach (JsonProperty property in root.EnumerateObject())
                {
                    string key = property.Name;
                    JsonElement value = property.Value;

                    filter.Eq(key, value);
                }
            }
            return filter;
        }


        public static ElasticMap getInstance() { return _Instance;  }
        private static ElasticMap _Instance = new ElasticMap();
        private ElasticMap() { }
        
    }
}
