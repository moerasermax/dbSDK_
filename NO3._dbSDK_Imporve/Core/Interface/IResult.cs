using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Core.Interface
{
    public interface IResult
    {
        bool IsSuccess { get; }
        string Msg { get; }
        string DataJson { get; }
        IResult setResult(bool success, string Msg);
        IResult setResult(bool success, string Msg,  string DataJson);
        IResult setErrorResult(string MethodName, string msg);
    }
}
