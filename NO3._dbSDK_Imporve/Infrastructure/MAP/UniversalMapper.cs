using AutoMapper;
using Microsoft.Extensions.Logging;
using NO3._dbSDK_Imporve.Core.Interface;
using System.Collections.Concurrent;

namespace NO3._dbSDK_Imporve.Infrastructure.MAP
{
    public class UniversalMapper : IUniversalMapper
    {
        // 快取池：記住已經建立過的對應關係，避免每次 Map 都重新消耗效能建立 Configuration
        private static readonly ConcurrentDictionary<(Type, Type), IMapper> _mapperCache = new();

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            if (source == null) return default;

            var sourceType = typeof(TSource);
            var destType = typeof(TDestination);
            var key = (sourceType, destType);

            // GetOrAdd：如果快取裡沒有這組 A->B，就當場動態建立並快取起來
            var mapper = _mapperCache.GetOrAdd(key, _ =>
            {
                // 🔥 這是 AutoMapper 16.1.1 的正確寫法 🔥
                // 必須先建立 MapperConfigurationExpression 物件
                var expr = new MapperConfigurationExpression();

                // 告訴它：請幫我建立這兩個型別的對應 (同名欄位自動 Map)
                expr.CreateMap(sourceType, destType);

                // 將 Expression 傳入 MapperConfiguration 的建構式中
                var config = new MapperConfiguration(expr, new LoggerFactory());

                return config.CreateMapper();
            });

            // 執行轉換
            return mapper.Map<TSource, TDestination>(source);
        }
    }


    public class LoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {

        }

        public ILogger CreateLogger(string categoryName)
        {

            return new logger();
        }

        public void Dispose()
        {

        }
    }
    public class logger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return true;
                case LogLevel.Debug:
                    return true;
                case LogLevel.Information:
                    return true;
                case LogLevel.Warning:
                    return true;
                case LogLevel.Error:
                    return true;
                case LogLevel.Critical:
                    return true;
                case LogLevel.None:
                    return true;
                default:
                    return true;
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Console.WriteLine($"{logLevel} {eventId} {state}");
        }
    }
}
