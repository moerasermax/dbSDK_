using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Core.Interface
{  
    public interface IUniversalMapper
    {
        TDestination Map<TSource, TDestination>(TSource source);
    }
}
