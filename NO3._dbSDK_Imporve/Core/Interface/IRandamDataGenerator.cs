
namespace NO3._dbSDK_Imporve.Core.Interface
{
    public interface IRandamDataGenerator<T, T1>
    {
        T Generate();
        List<T> Generate(int count);
        T CreateRandomItem();
        T1 ToSummary(T full);
        string GetRandomFrom(params string[] options);
    }
}
