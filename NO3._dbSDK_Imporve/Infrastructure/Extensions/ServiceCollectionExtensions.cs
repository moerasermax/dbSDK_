using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.DTO;
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
    }
}
