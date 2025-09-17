using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Tests.Data;
using Tests.Fixtures;

namespace Tests;

public interface ITestsColumnFilters<TFixture> where TFixture : ITestDbFixture
{
    TFixture Fixture { get; }

    public static List<TheoryDataRow<string, FilterOperations>> ValidIntCases()
    {
        return Utils.GetValidIntCases();
    }

    public static List<TheoryDataRow<string, FilterOperations>> ValidIntCasesWithProjection()
    {
        return Utils.GetValidIntCases();
    }

    public static List<TheoryDataRow<string, FilterOperations, FilterCategory, NumericColumn?>> InvalidFilterOperationsCombinations()
    {
        List<TheoryDataRow<string, FilterOperations, FilterCategory, NumericColumn?>> rows = [];
        FilterOperations[] textOnlyOperations = Utils.s_textOps[..^2]; // exclude Equals and NotEqual
        foreach (FilterOperations op in textOnlyOperations)
        {
            rows.Add((nameof(TestEntity.IntVal), op, FilterCategory.Numeric, NumericColumn.Int));
            rows.Add((nameof(TestEntity.NullableInt), op, FilterCategory.Numeric, NumericColumn.Int));
            rows.Add((nameof(TestEntity.DecimalVal), op, FilterCategory.Numeric, NumericColumn.Decimal));
            rows.Add((nameof(TestEntity.NullableDecimal), op, FilterCategory.Numeric, NumericColumn.Decimal));
            rows.Add((nameof(TestEntity.DateTimeVal), op, FilterCategory.Date, null));
            rows.Add((nameof(TestEntity.NullableDateTime), op, FilterCategory.Date, null));
            rows.Add((nameof(TestEntity.DateOnlyVal), op, FilterCategory.Date, null));
            rows.Add((nameof(TestEntity.NullableDateOnly), op, FilterCategory.Date, null));
        }

        FilterOperations[] numericOnlyOperations = Utils.s_numOps[2..]; // exclude Equals and NotEqual
        foreach (FilterOperations op in numericOnlyOperations)
        {
            rows.Add((nameof(TestEntity.StringVal), op, FilterCategory.Text, null));
            rows.Add((nameof(TestEntity.NullableString), op, FilterCategory.Text, null));
        }
        return rows;
    }

    public static List<TheoryDataRow<string, NumericColumn>> InvalidNumericColumnTypeCombinations()
    {
        List<TheoryDataRow<string, NumericColumn>> rows = [];

        rows.Add((nameof(TestEntity.IntVal), NumericColumn.Decimal));
        rows.Add((nameof(TestEntity.NullableInt), NumericColumn.Decimal));
        rows.Add((nameof(TestEntity.DecimalVal), NumericColumn.Int));
        rows.Add((nameof(TestEntity.NullableDecimal), NumericColumn.Int));

        return rows;
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(InvalidFilterOperationsCombinations))]
    public async Task InvalidFilterOperation_ShouldThrow(string propName, FilterOperations operation, FilterCategory category, NumericColumn? numericColumnType)
    {
        // Arrange
        var form = TestFormBuilder.Create();
        switch (category)
        {
            case FilterCategory.Text:
                form.AddColumn(propName, new TextFilterModel { SearchValue = "test", FilterType = operation, TextCategory = TextColumn.Base });
                break;
            case FilterCategory.Date:
                form.AddColumn(propName, new DateFilterModel { SearchValue = "2023-01-01", FilterType = operation });
                break;
            case FilterCategory.Numeric:
                form.AddColumn(propName, new NumericFilterModel { SearchValue = "123", FilterType = operation, NumericCategory = numericColumnType!.Value });
                break;
        }
        IFormCollection builtForm = form.Build();
        using TestDbContext contextNew = Fixture.CreateContext();

        // Act
        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => contextNew.TestEntities.ForDataTable(builtForm)
                                                                                         .WithoutSorting()
                                                                                         .BuildAsync(TestContext.Current.CancellationToken));
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(InvalidNumericColumnTypeCombinations))]
    public async Task InvalidColumnType_ShouldThrow(string propName, NumericColumn numericColumnType)
    {
        // Arrange
        IFormCollection form = TestFormBuilder.Create().AddColumn(propName, new NumericFilterModel { SearchValue = "123", NumericCategory = numericColumnType }).Build();
        using TestDbContext contextNew = Fixture.CreateContext();

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => contextNew.TestEntities.ForDataTable(form)
                                                                                 .WithoutSorting()
                                                                                 .BuildAsync(TestContext.Current.CancellationToken));
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidIntCases))]
    public async Task Filter_ValidIntCases(string searchValue, FilterOperations operation)
    {
        // Arrange
        using TestDbContext context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        IQueryable<TestEntity> baseQuery = context.TestEntities.AsQueryable();
        List<TestEntity> entities;
        if (operation == FilterOperations.Between)
        {
            string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
            if (!string.IsNullOrEmpty(values[0]))
            {
                int parsedValFrom = int.Parse(values[0]);
                baseQuery = baseQuery.Where(x => x.IntVal >= parsedValFrom);
            }
            if (!string.IsNullOrEmpty(values[1]))
            {
                int parsedValTo = int.Parse(values[1]);
                baseQuery = baseQuery.Where(x => x.IntVal <= parsedValTo);
            }
        }
        else
        {
            int parsedVal = int.Parse(searchValue);
            baseQuery = operation switch
            {
                FilterOperations.Equals => baseQuery.Where(x => x.IntVal == parsedVal),
                FilterOperations.NotEqual => baseQuery.Where(x => x.IntVal != parsedVal),
                FilterOperations.GreaterThan => baseQuery.Where(x => x.IntVal > parsedVal),
                FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.IntVal >= parsedVal),
                FilterOperations.LessThan => baseQuery.Where(x => x.IntVal < parsedVal),
                FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.IntVal <= parsedVal),
                _ => throw new InvalidOperationException($"{operation} not supported")
            };
        }
        entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
        IFormCollection form = TestFormBuilder.Create()
                                              .AddColumn(nameof(TestEntity.IntVal), new NumericFilterModel { SearchValue = searchValue, NumericCategory = NumericColumn.Int, FilterType = operation })
                                              .Build();

        using TestDbContext contextNew = Fixture.CreateContext();
        // Act
        Response<TestEntity> result = await contextNew.TestEntities.ForDataTable(form)
                                                                   .WithoutSorting()
                                                                   .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidIntCases))]
    public async Task Filter_ValidIntWithProjectionCases(string searchValue, FilterOperations operation)
    {
        // Arrange
        using TestDbContext context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        IQueryable<TestDto> baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        List<TestDto> entities;
        if (operation == FilterOperations.Between)
        {
            string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
            if (!string.IsNullOrEmpty(values[0]))
            {
                int parsedValFrom = int.Parse(values[0]);
                baseQuery = baseQuery.Where(x => x.IntVal >= parsedValFrom);
            }
            if (!string.IsNullOrEmpty(values[1]))
            {
                int parsedValTo = int.Parse(values[1]);
                baseQuery = baseQuery.Where(x => x.IntVal <= parsedValTo);
            }
        }
        else
        {
            int parsedVal = int.Parse(searchValue);
            baseQuery = operation switch
            {
                FilterOperations.Equals => baseQuery.Where(x => x.IntVal == parsedVal),
                FilterOperations.NotEqual => baseQuery.Where(x => x.IntVal != parsedVal),
                FilterOperations.GreaterThan => baseQuery.Where(x => x.IntVal > parsedVal),
                FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.IntVal >= parsedVal),
                FilterOperations.LessThan => baseQuery.Where(x => x.IntVal < parsedVal),
                FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.IntVal <= parsedVal),
                _ => throw new InvalidOperationException($"{operation} not supported")
            };
        }
        entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
        IFormCollection form = TestFormBuilder.Create()
                                              .AddColumn(nameof(TestDto.IntVal), new NumericFilterModel { SearchValue = searchValue, NumericCategory = NumericColumn.Int, FilterType = operation })
                                              .Build();

        using TestDbContext contextNew = Fixture.CreateContext();
        // Act
        Response<TestDto> result = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                                .WithoutSorting()
                                                                .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidIntCases))]
    public async Task Filter_ValidNullableIntCases(string searchValue, FilterOperations operation)
    {
        // Arrange
        using TestDbContext context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        IQueryable<TestEntity> baseQuery = context.TestEntities.AsQueryable();
        List<TestEntity> entities;
        if (operation == FilterOperations.Between)
        {
            string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
            if (!string.IsNullOrEmpty(values[0]))
            {
                int parsedValFrom = int.Parse(values[0]);
                baseQuery = baseQuery.Where(x => x.NullableInt >= parsedValFrom);
            }
            if (!string.IsNullOrEmpty(values[1]))
            {
                int parsedValTo = int.Parse(values[1]);
                baseQuery = baseQuery.Where(x => x.NullableInt <= parsedValTo);
            }
        }
        else
        {
            int parsedVal = int.Parse(searchValue);
            baseQuery = operation switch
            {
                FilterOperations.Equals => baseQuery.Where(x => x.NullableInt == parsedVal),
                FilterOperations.NotEqual => baseQuery.Where(x => x.NullableInt != parsedVal),
                FilterOperations.GreaterThan => baseQuery.Where(x => x.NullableInt > parsedVal),
                FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.NullableInt >= parsedVal),
                FilterOperations.LessThan => baseQuery.Where(x => x.NullableInt < parsedVal),
                FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.NullableInt <= parsedVal),
                _ => throw new InvalidOperationException($"{operation} not supported")
            };
        }
        entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
        IFormCollection form = TestFormBuilder.Create()
                                              .AddColumn(nameof(TestEntity.NullableInt), new NumericFilterModel { SearchValue = searchValue, NumericCategory = NumericColumn.Int, FilterType = operation })
                                              .Build();

        using TestDbContext contextNew = Fixture.CreateContext();
        // Act
        Response<TestEntity> result = await contextNew.TestEntities.ForDataTable(form)
                                                                   .WithoutSorting()
                                                                   .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidIntCases))]
    public async Task Filter_ValidNullableIntWithProjectionCases(string searchValue, FilterOperations operation)
    {
        // Arrange
        using TestDbContext context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        IQueryable<TestDto> baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        List<TestDto> entities;
        if (operation == FilterOperations.Between)
        {
            string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
            if (!string.IsNullOrEmpty(values[0]))
            {
                int parsedValFrom = int.Parse(values[0]);
                baseQuery = baseQuery.Where(x => x.NullInt >= parsedValFrom);
            }
            if (!string.IsNullOrEmpty(values[1]))
            {
                int parsedValTo = int.Parse(values[1]);
                baseQuery = baseQuery.Where(x => x.NullInt <= parsedValTo);
            }
        }
        else
        {
            int parsedVal = int.Parse(searchValue);
            baseQuery = operation switch
            {
                FilterOperations.Equals => baseQuery.Where(x => x.NullInt == parsedVal),
                FilterOperations.NotEqual => baseQuery.Where(x => x.NullInt != parsedVal),
                FilterOperations.GreaterThan => baseQuery.Where(x => x.NullInt > parsedVal),
                FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.NullInt >= parsedVal),
                FilterOperations.LessThan => baseQuery.Where(x => x.NullInt < parsedVal),
                FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.NullInt <= parsedVal),
                _ => throw new InvalidOperationException($"{operation} not supported")
            };
        }
        entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
        IFormCollection form = TestFormBuilder.Create()
                                              .AddColumn(nameof(TestDto.NullInt), new NumericFilterModel { SearchValue = searchValue, NumericCategory = NumericColumn.Int, FilterType = operation })
                                              .Build();



        using TestDbContext contextNew = Fixture.CreateContext();
        // Act
        Response<TestDto> result = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                                .WithoutSorting()
                                                                .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }
}

