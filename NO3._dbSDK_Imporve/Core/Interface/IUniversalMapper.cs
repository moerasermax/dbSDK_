namespace NO3._dbSDK_Imporve.Core.Interface
{  
    public interface IUniversalMapper
    {
        TDestination Map<TSource, TDestination>(TSource source);
    }
}
