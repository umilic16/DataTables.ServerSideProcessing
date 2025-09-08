using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Tests.Data;

namespace Tests.Fixtures;
public class MsSqlFixture : IAsyncLifetime, ITestDbFixture
{
    private readonly MsSqlContainer _container = new MsSqlBuilder().Build();
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlServer(ConnectionString).Options;
        DbHelpers.InitializeDatabase(options);
    }
    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
    public TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlServer(ConnectionString).Options;
        return new TestDbContext(options);
    }
    public string ConnectionString => _container.GetConnectionString();
}

