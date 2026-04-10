using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;

namespace NO3._dbSDK_Imporve.Infrastructure.External
{
    public abstract class BaseRandomDataGenerator<T>: IRandamDataGenerator<T>
    {

        public static readonly Random _random = new Random();

        public T Generate()
        {
            return CreateRandomItem();
        }

        public List<T> Generate(int count)
        {
            var list = new List<T>();

            for (int i = 0; i < count; i++)
            {
                var item = CreateRandomItem();
                list.Add(item);
            }

            return list;
        }

        public abstract T CreateRandomItem();

        // 共享的工具方法
        public string GetRandomFrom(params string[] options)
        {
            return options[_random.Next(options.Length)];
        }

        // 可以加入通用的數字、日期隨機生成邏輯
        protected int NextInt(int min, int max) => _random.Next(min, max);

        protected DateTime NextDateTime(int daysInPast = 30)
        {
            return DateTime.UtcNow.AddDays(-NextInt(0, daysInPast)).AddMinutes(-NextInt(0, 1440));
        }
        protected bool NextBool() => _random.Next(2) == 0;
    }
}
