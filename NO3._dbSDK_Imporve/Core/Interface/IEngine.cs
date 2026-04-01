using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Core.Interface
{
    public interface IEngine<T>
    {
        Task Insert(T Data);
        Task Update(string conditionData, T Data);
        Task Read(string conditionData);
        Task Remove(string conditionData);
    }
}
