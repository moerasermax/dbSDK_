using Microsoft.Extensions.Logging;

namespace NO3._dbSDK_Imporve.Core.Models
{
    public class Condition
    {
        public string event_id { get; }
        public Condition(string Event_id)
        {
            event_id = Event_id;
        }
    }
}
