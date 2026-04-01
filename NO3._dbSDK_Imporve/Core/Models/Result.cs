using NO3._dbSDK_Imporve.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Core.Models
{
    internal class Result : IResult
    {
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }
        public string DataJson { get; set; }

        public IResult setResult(bool isSuccess, string msg)
        {
            this.IsSuccess = isSuccess;
            this.Msg = msg;
            return this;
        }
        public IResult setResult(bool isSuccess, string msg, string data)
        {
            this.IsSuccess = isSuccess;
            this.Msg = msg;
            this.DataJson = data;
            return this;
        }

        public IResult setErrorResult(string MethodName, string msg)
        {
            this.IsSuccess = false;
            this.Msg = string.Format("發生錯誤請檢查，函示名稱{0}\r\n錯誤訊息：{1}", MethodName, msg);
            return this;
        }

        public IResult clearData()
        {
            this.DataJson = string.Empty;
            return this;
        }
    }
}
