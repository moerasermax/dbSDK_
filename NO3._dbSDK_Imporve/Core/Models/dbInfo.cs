using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Core.Models
{
    public class dbInfo
    {
        public string Uri { get; set; }
        public string User { get; set; }
        public string Password { get; set; }   
        public string EndPoint { get; set; }
        public string ApiKey { get; set;  }
        public dbInfo setInfo(string uri, string user, string password)
        {
            this.Uri = uri;
            this.User = user;
            this.Password = password;
            return this;
        }
        public dbInfo setEndPoint(string _EndPoint)
        {
            this.EndPoint = _EndPoint; 
            return this;
        }
        public dbInfo setApiKey(string _ApiKey)
        {
            ApiKey = _ApiKey;
            return this;
        }
    }
}
