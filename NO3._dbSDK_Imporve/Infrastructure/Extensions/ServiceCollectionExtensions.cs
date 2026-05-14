using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.DTO;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DbSdkServiceCollectionExtensions
    {
        public const string DefaultConfigSectionName = "ConnectionSettings";

        public static IServiceCollection AddDbSdk(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = DefaultConfigSectionName)
        {
            services.Configure<ConnectionSettings>(configuration.GetSection(sectionName));

            services.AddSingleton<MongoDBDriver>(sp =>
                new MongoDBDriver("MongoDB", sp.GetRequiredService<IOptions<ConnectionSettings>>()));
            services.AddSingleton<ElasticDriver>(sp =>
                new ElasticDriver("Elastic", sp.GetRequiredService<IOptions<ConnectionSettings>>()));
            services.AddSingleton<RedisDriver>(sp =>
                new RedisDriver("Redis", sp.GetRequiredService<IOptions<ConnectionSettings>>()));

            services.AddSingleton<MongoMap>();
            services.AddSingleton<ElasticMap>();
            services.AddSingleton<IDTO, DTO>();

            return services;
        }

        public static IServiceCollection AddDbSdkMongoRepository<TModel>(
            this IServiceCollection services,
            string collectionName)
        {
            services.AddSingleton<IMongoDBRepository<TModel>>(sp =>
                new MongoRepository<TModel>(
                    sp.GetRequiredService<MongoDBDriver>(),
                    sp.GetRequiredService<MongoMap>(),
                    sp.GetRequiredService<IDTO>(),
                    collectionName));

            return services;
        }

        public static IServiceCollection AddDbSdkElasticRepository<TModel>(
            this IServiceCollection services,
            string indexPattern)
        {
            // 註冊 concrete class — 給 OrderSearchDal 等吃強型別 ctor 的 caller 用
            services.AddSingleton<ElasticRepository<TModel>>(sp =>
                new ElasticRepository<TModel>(
                    sp.GetRequiredService<ElasticDriver>(),
                    sp.GetRequiredService<ElasticMap>(),
                    indexPattern));

            // Forward IRepository<TModel> 到同一實例 — 給吃介面的 caller 用 (例:ShippingSyncService)
            services.AddSingleton<IRepository<TModel>>(sp =>
                sp.GetRequiredService<ElasticRepository<TModel>>());

            return services;
        }
    }
}
