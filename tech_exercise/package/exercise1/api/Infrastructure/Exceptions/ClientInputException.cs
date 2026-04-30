namespace StargateAPI.Infrastructure.Exceptions
{
    // Created to help distinguish between client input errors and server errors
    public class ClientInputException : Exception
    {
        public ClientInputException(string message) : base(message)
        {
        }
    }
}
