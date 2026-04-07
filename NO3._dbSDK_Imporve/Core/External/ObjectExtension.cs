namespace NO3._dbSDK_Imporve.Core.External
{
    public class ObjectExtension
    {
        
        public ObjectExtension() 
        {
            
        }

        public T Copy<T>(T source)
        {
            if (source == null)
            {
                return default(T);
            }
            var serialized = System.Text.Json.JsonSerializer.Serialize(source);
            return System.Text.Json.JsonSerializer.Deserialize<T>(serialized);
        }
    }
}
