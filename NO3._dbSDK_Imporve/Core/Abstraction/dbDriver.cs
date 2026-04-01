using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Core.Abstraction
{
    public abstract class dbDriver : IdbDriver
    {
        public string _Service { get;}
        public dbDriver(string Service)
        {
            _Service = Service;
        }
    }
}
