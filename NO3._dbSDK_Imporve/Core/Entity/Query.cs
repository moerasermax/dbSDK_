
namespace NO3._dbSDK_Imporve.Core.Entity
{
    public class Query
    {
        public Query() { }
        public Query(string _QueryDB) 
        {
            QueryDB = _QueryDB;
        }
        public string QueryDB { get;  }
        public string QueryID { get; set; }
        public string QueryType { get; set;  }
        public string OrderData { get; set; }
        public string CreateTime { get; set; }
    }
}
