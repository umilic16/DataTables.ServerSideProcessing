using Tests.Fixtures;

namespace Tests;

public class PostgreSqlTests(PostgreSqlFixture fixture)
    : TestsBase<PostgreSqlFixture>(fixture), IClassFixture<PostgreSqlFixture>;

public class PomeloTests(PomeloFixture fixture)
    : TestsBase<PomeloFixture>(fixture), IClassFixture<PomeloFixture>;

public class MySqlTests(MySqlFixture fixture)
    : TestsBase<MySqlFixture>(fixture), IClassFixture<MySqlFixture>;

public class MsSqlTests(MsSqlFixture fixture)
    : TestsBase<MsSqlFixture>(fixture), IClassFixture<MsSqlFixture>;
