using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace CustomerAdmin.Api.Tests.Infrastructure;

/// <summary>
/// Starts a real PostgreSQL in Docker once for all tests and runs the API in memory against it.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18").Build();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        // The API reads the connection string while it starts. An environment variable is the
        // simplest way to hand it over, and it takes priority over user secrets.
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _database.GetConnectionString());
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}