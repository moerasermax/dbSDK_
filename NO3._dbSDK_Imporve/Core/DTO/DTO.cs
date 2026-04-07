using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;

namespace NO3._dbSDK_Imporve.Core.DTO
{
    public class DTO : IDTO
    {
        public Condition GetCondition(string id)
        {
            return new Condition(id);
        }
    }
}
