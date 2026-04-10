using Microsoft.Extensions.Logging;

namespace NO3._dbSDK_Imporve.Core.Models
{
    public class Condition
    {
        public string _coom_no { get; }
        public Condition(string coom_no)
        {
            _coom_no = coom_no;
        }
    }
}
