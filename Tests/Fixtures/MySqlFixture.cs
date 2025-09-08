using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;
using Tests.Data;

namespace Tests.Fixtures;
public class MySqlFixture : IAsyncLifetime, ITestDbFixture
{
    private readonly MySqlContainer _container = new MySqlBuilder().Build();
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseMySQL(ConnectionString).Options;
        DbHelpers.InitializeDatabase(options);
    }
    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
    public TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseMySQL(ConnectionString).Options;
        return new TestDbContext(options);
    }
    public string ConnectionString => _container.GetConnectionString();
}

