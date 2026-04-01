using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Core.Interface
{
    public interface IRepository<T>
    {
        IResult Result { get; set; }
        Task<IResult> insertData(T Data);
        Task<IResult> removeData(string ConditionData);
        Task<IResult> updateData(string ConditionData, T UpdateData);
        Task<IResult> getData(string ConditionData);
    }
}
