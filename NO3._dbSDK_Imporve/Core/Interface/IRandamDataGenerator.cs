
namespace NO3._dbSDK_Imporve.Core.Interface
{
    public interface IRandamDataGenerator<T>
    {
        T Generate();
        List<T> Generate(int count);
        T CreateRandomItem();
    }
}
