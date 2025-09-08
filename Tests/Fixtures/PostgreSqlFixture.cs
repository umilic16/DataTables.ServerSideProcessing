using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Tests.Data;

namespace Tests.Fixtures;
public class PostgreSqlFixture : IAsyncLifetime, ITestDbFixture
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder().Build();
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseNpgsql(ConnectionString).Options;
        DbHelpers.InitializeDatabase(options);
    }
    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
    public TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseNpgsql(ConnectionString).Options;
        return new TestDbContext(options);
    }
    public string ConnectionString => _container.GetConnectionString();
}

