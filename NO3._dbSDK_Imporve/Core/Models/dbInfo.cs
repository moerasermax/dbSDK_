
namespace NO3._dbSDK_Imporve.Core.Configurations
{
    /// <summary>
    /// 總體連線配置容器
    /// </summary>
    public class ConnectionSettings
    {
        public DbDetail Mongo { get; set; } = new();
        public DbDetail Elastic { get; set; } = new();
        public DbDetail Redis { get; set; } = new();
    }

    /// <summary>
    /// 通用資料庫細節模型
    /// </summary>
    public class DbDetail
    {
        public string Uri { get; set; }
        public string EndPoint { get; set; }
        public int Port { get; set; } // 新增：支援 Redis 等需要 Port 的情境
        public string User { get; set; }
        public string Password { get; set; }
        public string ApiKey { get; set; }
    }
}
