
namespace NO3._dbSDK_Imporve.Core.Entity
{
    public class Query
    {
        public string? Name { get; set; } = null;

    }

    public class TestQuery: Query
    {
        public TestQuery(string _queryDB)
        {
            QueryDB = _queryDB;
        }
        public string QueryDB { get; private set; }
        public string QueryID { get; set; }
        public string QueryType { get; set; }
        public string OrderData { get; set; }
        public string CreateTime { get; set; }
    }
}
