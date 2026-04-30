namespace StargateAPI.Infrastructure.Logging
{
    public interface IProcessLogWriter
    {
        Task LogAsync(
            string level,
            string message,
            string path,
            string method,
            int statusCode,
            CancellationToken cancellationToken = default);
    }
}
