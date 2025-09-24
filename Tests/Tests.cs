using Tests.Fixtures;

namespace Tests;

public class PostgreSqlTests(PostgreSqlFixture fixture) : ITestsSorting<PostgreSqlFixture>, ITestsColumnFilters<PostgreSqlFixture>, ITestsGlobalFilter<PostgreSqlFixture>,
    ITestsSingleSelectFilter<PostgreSqlFixture>, ITestsMultiSelectFilter<PostgreSqlFixture>, IClassFixture<PostgreSqlFixture>
{
    public PostgreSqlFixture Fixture => fixture;
    public bool IsPostgres => true;
}

public class PomeloTests(PomeloFixture fixture) : ITestsSorting<PomeloFixture>, ITestsColumnFilters<PomeloFixture>, ITestsGlobalFilter<PomeloFixture>,
    ITestsSingleSelectFilter<PomeloFixture>, ITestsMultiSelectFilter<PomeloFixture>, IClassFixture<PomeloFixture>
{
    public PomeloFixture Fixture => fixture;
    public bool IsPostgres => false;
}

public class MySqlTests(MySqlFixture fixture) : ITestsSorting<MySqlFixture>, ITestsColumnFilters<MySqlFixture>, ITestsGlobalFilter<MySqlFixture>,
    ITestsSingleSelectFilter<MySqlFixture>, ITestsMultiSelectFilter<MySqlFixture>, IClassFixture<MySqlFixture>
{
    public MySqlFixture Fixture => fixture;
    public bool IsPostgres => false;
}

public class MsSqlTests(MsSqlFixture fixture) : ITestsSorting<MsSqlFixture>, ITestsColumnFilters<MsSqlFixture>, ITestsGlobalFilter<MsSqlFixture>, ITestsSingleSelectFilter<MsSqlFixture>,
    ITestsMultiSelectFilter<MsSqlFixture>, IClassFixture<MsSqlFixture>
{
    public MsSqlFixture Fixture => fixture;
    public bool IsPostgres => false;
}
