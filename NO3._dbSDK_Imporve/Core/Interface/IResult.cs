namespace NO3._dbSDK_Imporve.Core.Interface
{
    public interface IResult
    {
        bool IsSuccess { get; }
        string Msg { get; }
        string DataJson { get; }
    }
}
