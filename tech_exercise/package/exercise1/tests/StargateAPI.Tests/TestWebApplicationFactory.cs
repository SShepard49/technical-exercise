using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace StargateAPI.Tests;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"stargate-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        var connectionString = $"Data Source={_dbPath}";
        // UseSetting ensures the value is present before host configuration merges with appsettings.
        builder.UseSetting("ConnectionStrings:StarbaseApiDatabase", connectionString);
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:StarbaseApiDatabase"] = connectionString
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        SqliteConnection.ClearAllPools();
        TryDelete(_dbPath);
        TryDelete(_dbPath + "-wal");
        TryDelete(_dbPath + "-shm");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
        {
            return;
        }

        SqliteConnection.ClearAllPools();
        TryDelete(_dbPath);
        TryDelete(_dbPath + "-wal");
        TryDelete(_dbPath + "-shm");
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
