using NO3._dbSDK_Imporve.Core.Interface;


namespace NO3._dbSDK_Imporve.Core.Models
{
    public class Result : IResult
    {
        public bool IsSuccess { get;  }
        public string Msg { get;  }
        public string DataJson { get; }

        private Result(bool isSuccess, string msg, string datajson) 
        {
            IsSuccess = isSuccess;
            Msg = msg;
            DataJson = datajson;
        }
        public static Result setResult(string msg, bool isSuccess = true) => new(isSuccess, msg, "");
        public static Result setResult(string msg, string DataJson, bool isSuccess = true) => new(isSuccess, msg, DataJson);
        public static Result setErrorResult(string MethodName, string msg, bool isSucess = false) => new(isSucess, string.Format("發生錯誤請檢查，函示名稱{0}\r\n錯誤訊息：{1}",MethodName,msg), "");
        }


}
