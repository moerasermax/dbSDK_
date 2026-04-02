
namespace NO3._dbSDK_Imporve.Core.Abstraction
{
    public abstract class BaseRandomDataGenerator
    {
        private static readonly Random _random = new Random();

        // 共享的工具方法
        public string GetRandomFrom(params string[] options)
        {
            return options[_random.Next(options.Length)];
        }

        // 可以加入通用的數字、日期隨機生成邏輯
        protected int NextInt(int min, int max) => _random.Next(min, max);
    }
}
