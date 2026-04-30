using StargateAPI.Business.Data;

namespace StargateAPI.Infrastructure.Logging
{
    public class ProcessLogWriter : IProcessLogWriter
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ProcessLogWriter(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task LogAsync(
            string level,
            string message,
            string path,
            string method,
            int statusCode,
            CancellationToken cancellationToken = default)
        {
            //Create a new context to avoid potential context reuse issues
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<StargateContext>();

            var entry = new ProcessLog
            {
                OccurredAtUtc = DateTimeOffset.UtcNow,
                Level = level,
                Message = message,
                Path = path,
                Method = method,
                StatusCode = statusCode
            };

            context.ProcessLogs.Add(entry);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
