using Tests.Data;

namespace Tests.Fixtures;
public interface ITestDbFixture
{
    TestDbContext CreateContext();
}
