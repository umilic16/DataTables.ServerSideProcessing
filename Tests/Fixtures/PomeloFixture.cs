using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;
using Tests.Data;

namespace Tests.Fixtures;

public class PomeloFixture : IAsyncLifetime, ITestDbFixture
{
    private readonly MySqlContainer _container = new MySqlBuilder().Build();
    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString)).Options;
        DbHelpers.InitializeDatabase(options);
    }
    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
        GC.SuppressFinalize(this);
    }
    public TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString)).Options;
        return new TestDbContext(options);
    }
    public string ConnectionString => _container.GetConnectionString();
}

