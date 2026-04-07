using NO3._dbSDK_Imporve.Core.Interface;
namespace NO3._dbSDK_Imporve.Core.Abstraction
{
    public abstract class DbDriver : IdbDriver
    {
        public string _Service { get;}
        public DbDriver(string Service)
        {
            _Service = Service;
        }
    }
}
