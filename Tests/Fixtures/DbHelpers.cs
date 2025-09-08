using Microsoft.EntityFrameworkCore;
using Tests.Data;

namespace Tests.Fixtures;

public static class DbHelpers
{
    public static void InitializeDatabase(DbContextOptions<TestDbContext> options)
    {
        using var context = new TestDbContext(options);
        context.Database.EnsureCreated();  // or Migrate()
        context.TestEntities.RemoveRange(context.TestEntities); // clear if needed
        var now = DateTime.UtcNow;
        context.TestEntities.AddRange(
            new TestEntity
            {
                IntVal = 1,
                NullableInt = null,
                DecimalVal = 1.23m,
                NullableDecimal = null,
                DateTimeVal = now,
                NullableDateTime = null,
                DateOnlyVal = DateOnly.FromDateTime(now),
                NullableDateOnly = null,
                EnumVal = Something.Alpha,
                NullableEnum = null,
                BoolVal = true,
                NullableBool = null,
                StringVal = "test",
                NullableString = null
            },
            new TestEntity
            {
                IntVal = 2,
                NullableInt = 3,
                DecimalVal = -16.22m,
                NullableDecimal = -18000.018m,
                DateTimeVal = now,
                NullableDateTime = now,
                DateOnlyVal = DateOnly.FromDateTime(now),
                NullableDateOnly = DateOnly.FromDateTime(now),
                EnumVal = Something.Gamma,
                NullableEnum = Something.Alpha,
                BoolVal = true,
                NullableBool = false,
                StringVal = "test 123",
                NullableString = "test 456"
            },
            new TestEntity
            {
                IntVal = int.MaxValue,
                NullableInt = int.MinValue,
                DecimalVal = -16_022_111_998.018m,
                NullableDecimal = 22_111_998.018m,
                DateTimeVal = DateTime.MinValue,
                NullableDateTime = DateTime.MaxValue,
                DateOnlyVal = DateOnly.MinValue,
                NullableDateOnly = DateOnly.MaxValue,
                EnumVal = Something.Gamma,
                NullableEnum = null,
                BoolVal = true,
                NullableBool = false,
                StringVal = "test 123",
                NullableString = """`1234567890-=[]\;',./~!@#$%^&*()_+{}|:"<>?"""
            },
            new TestEntity
            {
                IntVal = 123,
                NullableInt = null,
                DecimalVal = 123,
                NullableDecimal = null,
                DateTimeVal = now.AddDays(655),
                NullableDateTime = now.AddMonths(-45),
                DateOnlyVal = DateOnly.FromDateTime(now.AddYears(5)),
                NullableDateOnly = DateOnly.FromDateTime(now.AddDays(-18000)),
                EnumVal = Something.Alpha,
                NullableEnum = null,
                BoolVal = true,
                NullableBool = null,
                StringVal = "qwertyuiopasdfghjklzxcvbnm",
                NullableString = null
            },
            new TestEntity
            {
                IntVal = 123,
                NullableInt = null,
                DecimalVal = 123.164646m,
                NullableDecimal = -321.0054m,
                DateTimeVal = now.AddDays(125),
                NullableDateTime = now.AddYears(5),
                DateOnlyVal = DateOnly.FromDateTime(now.AddYears(5)),
                NullableDateOnly = DateOnly.FromDateTime(now.AddDays(-18000)),
                EnumVal = Something.Gamma,
                NullableEnum = Something.Beta,
                BoolVal = true,
                NullableBool = false,
                StringVal = "test    test     test \r\ntest \ttest\rtest\t\ntest123",
                NullableString = "something"
            },
            new TestEntity
            {
                IntVal = 1,
                NullableInt = null,
                DecimalVal = 10.5m,
                NullableDecimal = null,
                DateTimeVal = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                NullableDateTime = null,
                DateOnlyVal = new DateOnly(2021, 6, 1),
                NullableDateOnly = null,
                EnumVal = Something.Alpha,
                NullableEnum = null,
                BoolVal = true,
                NullableBool = null,
                StringVal = "Alice",
                NullableString = null
            },
            new TestEntity
            {
                IntVal = 2,
                NullableInt = 5,
                DecimalVal = 20.5m,
                NullableDecimal = 2.5m,
                DateTimeVal = new DateTime(2022, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                NullableDateTime = new DateTime(2022, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                DateOnlyVal = new DateOnly(2021, 7, 1),
                NullableDateOnly = new DateOnly(2022, 8, 8),
                EnumVal = Something.Beta,
                NullableEnum = Something.Alpha,
                BoolVal = false,
                NullableBool = true,
                StringVal = "Bob",
                NullableString = "Beta"
            },
            new TestEntity
            {
                IntVal = 3,
                NullableInt = 5,
                DecimalVal = 30.5m,
                NullableDecimal = 3.5m,
                DateTimeVal = new DateTime(2023, 3, 3, 0, 0, 0, DateTimeKind.Utc),
                NullableDateTime = new DateTime(2023, 11, 11, 0, 0, 0, DateTimeKind.Utc),
                DateOnlyVal = new DateOnly(2021, 8, 1),
                NullableDateOnly = new DateOnly(2023, 9, 9),
                EnumVal = Something.Gamma,
                NullableEnum = Something.Beta,
                BoolVal = true,
                NullableBool = false,
                StringVal = "Charlie",
                NullableString = "Gamma"
            }
        );
        context.SaveChanges();
    }
}
