using System.Globalization;
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

    public static List<TheoryDataRow<string, FilterOperations, string>> ValidDecCases()
    {
        return Utils.GetValidDecCases();
    }

    public static List<TheoryDataRow<string, FilterOperations, string>> ValidDateCases()
    {
        return Utils.GetValidDateCases();
    }

    public static List<TheoryDataRow<string, FilterOperations>> ValidStringCases()
    {
        return Utils.GetValidStringCases();
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
    public async Task Filter_ValidInt(string searchValue, FilterOperations operation)
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
            if (!string.IsNullOrEmpty(searchValue))
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
    public async Task Filter_ValidInt_WithProjection(string searchValue, FilterOperations operation)
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
            if (!string.IsNullOrEmpty(searchValue))
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
    public async Task Filter_ValidNullableInt(string searchValue, FilterOperations operation)
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
            if (!string.IsNullOrEmpty(searchValue))
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
    public async Task Filter_ValidNullableInt_WithProjection(string searchValue, FilterOperations operation)
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
            if (!string.IsNullOrEmpty(searchValue))
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

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidDecCases))]
    public async Task Filter_ValidDecimal(string searchValue, FilterOperations operation, string culture)
    {
        // Arrange
        var oldCulture = CultureInfo.CurrentCulture;
        var newCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = newCulture;
        try
        {
            using TestDbContext context = Fixture.CreateContext();
            int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
            IQueryable<TestEntity> baseQuery = context.TestEntities.AsQueryable();
            List<TestEntity> entities;
            if (operation == FilterOperations.Between)
            {
                string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
                if (!string.IsNullOrEmpty(values[0]))
                {
                    decimal parsedValFrom = decimal.Parse(values[0], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.DecimalVal >= parsedValFrom);
                }
                if (!string.IsNullOrEmpty(values[1]))
                {
                    decimal parsedValTo = decimal.Parse(values[1], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.DecimalVal <= parsedValTo);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    decimal parsedVal = decimal.Parse(searchValue, CultureInfo.CurrentCulture);
                    baseQuery = operation switch
                    {
                        FilterOperations.Equals => baseQuery.Where(x => x.DecimalVal == parsedVal),
                        FilterOperations.NotEqual => baseQuery.Where(x => x.DecimalVal != parsedVal),
                        FilterOperations.GreaterThan => baseQuery.Where(x => x.DecimalVal > parsedVal),
                        FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.DecimalVal >= parsedVal),
                        FilterOperations.LessThan => baseQuery.Where(x => x.DecimalVal < parsedVal),
                        FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.DecimalVal <= parsedVal),
                        _ => throw new InvalidOperationException($"{operation} not supported")
                    };
                }
            }
            entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
            IFormCollection form = TestFormBuilder.Create()
                                                  .AddColumn(nameof(TestEntity.DecimalVal), new NumericFilterModel { SearchValue = searchValue, NumericCategory = NumericColumn.Decimal, FilterType = operation })
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
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidDecCases))]
    public async Task Filter_ValidDecimal_WithProjection(string searchValue, FilterOperations operation, string culture)
    {
        // Arrange
        var oldCulture = CultureInfo.CurrentCulture;
        var newCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = newCulture;
        try
        {
            using TestDbContext context = Fixture.CreateContext();
            int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
            IQueryable<TestDto> baseQuery = context.TestEntities.Select(Mappings.SelectDto);
            List<TestDto> entities;
            if (operation == FilterOperations.Between)
            {
                string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
                if (!string.IsNullOrEmpty(values[0]))
                {
                    decimal parsedValFrom = decimal.Parse(values[0], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.DecVal >= parsedValFrom);
                }
                if (!string.IsNullOrEmpty(values[1]))
                {
                    decimal parsedValTo = decimal.Parse(values[1], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.DecVal <= parsedValTo);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    decimal parsedVal = decimal.Parse(searchValue, CultureInfo.CurrentCulture);
                    baseQuery = operation switch
                    {
                        FilterOperations.Equals => baseQuery.Where(x => x.DecVal == parsedVal),
                        FilterOperations.NotEqual => baseQuery.Where(x => x.DecVal != parsedVal),
                        FilterOperations.GreaterThan => baseQuery.Where(x => x.DecVal > parsedVal),
                        FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.DecVal >= parsedVal),
                        FilterOperations.LessThan => baseQuery.Where(x => x.DecVal < parsedVal),
                        FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.DecVal <= parsedVal),
                        _ => throw new InvalidOperationException($"{operation} not supported")
                    };
                }
            }
            entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
            IFormCollection form = TestFormBuilder.Create()
                                                  .AddColumn(nameof(TestDto.DecVal), new NumericFilterModel { SearchValue = searchValue, NumericCategory = NumericColumn.Decimal, FilterType = operation })
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
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidDecCases))]
    public async Task Filter_ValidNullableDecimal(string searchValue, FilterOperations operation, string culture)
    {
        // Arrange
        var oldCulture = CultureInfo.CurrentCulture;
        var newCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = newCulture;
        try
        {
            using TestDbContext context = Fixture.CreateContext();
            int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
            IQueryable<TestEntity> baseQuery = context.TestEntities.AsQueryable();
            List<TestEntity> entities;
            if (operation == FilterOperations.Between)
            {
                string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
                if (!string.IsNullOrEmpty(values[0]))
                {
                    decimal parsedValFrom = decimal.Parse(values[0], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.NullableDecimal >= parsedValFrom);
                }
                if (!string.IsNullOrEmpty(values[1]))
                {
                    decimal parsedValTo = decimal.Parse(values[1], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.NullableDecimal <= parsedValTo);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    decimal parsedVal = decimal.Parse(searchValue, CultureInfo.CurrentCulture);
                    baseQuery = operation switch
                    {
                        FilterOperations.Equals => baseQuery.Where(x => x.NullableDecimal == parsedVal),
                        FilterOperations.NotEqual => baseQuery.Where(x => x.NullableDecimal != parsedVal),
                        FilterOperations.GreaterThan => baseQuery.Where(x => x.NullableDecimal > parsedVal),
                        FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.NullableDecimal >= parsedVal),
                        FilterOperations.LessThan => baseQuery.Where(x => x.NullableDecimal < parsedVal),
                        FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.NullableDecimal <= parsedVal),
                        _ => throw new InvalidOperationException($"{operation} not supported")
                    };
                }
            }
            entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
            IFormCollection form = TestFormBuilder.Create()
                                                  .AddColumn(nameof(TestEntity.NullableDecimal), new NumericFilterModel { SearchValue = searchValue, NumericCategory = NumericColumn.Decimal, FilterType = operation })
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
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidDecCases))]
    public async Task Filter_ValidNullableDecimal_WithProjection(string searchValue, FilterOperations operation, string culture)
    {
        // Arrange
        var oldCulture = CultureInfo.CurrentCulture;
        var newCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = newCulture;
        try
        {
            using TestDbContext context = Fixture.CreateContext();
            int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
            IQueryable<TestDto> baseQuery = context.TestEntities.Select(Mappings.SelectDto);
            List<TestDto> entities;
            if (operation == FilterOperations.Between)
            {
                string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
                if (!string.IsNullOrEmpty(values[0]))
                {
                    decimal parsedValFrom = decimal.Parse(values[0], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.NullDec >= parsedValFrom);
                }
                if (!string.IsNullOrEmpty(values[1]))
                {
                    decimal parsedValTo = decimal.Parse(values[1], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.NullDec <= parsedValTo);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    decimal parsedVal = decimal.Parse(searchValue, CultureInfo.CurrentCulture);
                    baseQuery = operation switch
                    {
                        FilterOperations.Equals => baseQuery.Where(x => x.NullDec == parsedVal),
                        FilterOperations.NotEqual => baseQuery.Where(x => x.NullDec != parsedVal),
                        FilterOperations.GreaterThan => baseQuery.Where(x => x.NullDec > parsedVal),
                        FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.NullDec >= parsedVal),
                        FilterOperations.LessThan => baseQuery.Where(x => x.NullDec < parsedVal),
                        FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.NullDec <= parsedVal),
                        _ => throw new InvalidOperationException($"{operation} not supported")
                    };
                }
            }
            entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
            IFormCollection form = TestFormBuilder.Create()
                                                  .AddColumn(nameof(TestDto.NullDec), new NumericFilterModel { SearchValue = searchValue, NumericCategory = NumericColumn.Decimal, FilterType = operation })
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
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidDateCases))]
    public async Task Filter_ValidDateTime(string searchValue, FilterOperations operation, string culture)
    {
        // Arrange
        var oldCulture = CultureInfo.CurrentCulture;
        var newCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = newCulture;
        try
        {
            using TestDbContext context = Fixture.CreateContext();
            int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
            IQueryable<TestEntity> baseQuery = context.TestEntities.AsQueryable();
            List<TestEntity> entities;
            if (operation == FilterOperations.Between)
            {
                string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
                if (!string.IsNullOrEmpty(values[0]))
                {
                    DateTime parsedValFrom = DateTime.Parse(values[0], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.DateTimeVal >= parsedValFrom);
                }
                if (!string.IsNullOrEmpty(values[1]))
                {
                    DateTime parsedValTo = DateTime.Parse(values[1], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.DateTimeVal <= parsedValTo);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    DateTime parsedVal = DateTime.Parse(searchValue, CultureInfo.CurrentCulture);
                    baseQuery = operation switch
                    {
                        FilterOperations.Equals => baseQuery.Where(x => x.DateTimeVal == parsedVal),
                        FilterOperations.NotEqual => baseQuery.Where(x => x.DateTimeVal != parsedVal),
                        FilterOperations.GreaterThan => baseQuery.Where(x => x.DateTimeVal > parsedVal),
                        FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.DateTimeVal >= parsedVal),
                        FilterOperations.LessThan => baseQuery.Where(x => x.DateTimeVal < parsedVal),
                        FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.DateTimeVal <= parsedVal),
                        _ => throw new InvalidOperationException($"{operation} not supported")
                    };
                }
            }
            entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
            IFormCollection form = TestFormBuilder.Create()
                                                  .AddColumn(nameof(TestEntity.DateTimeVal), new DateFilterModel { SearchValue = searchValue, FilterType = operation })
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
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidDateCases))]
    public async Task Filter_ValidDateTime_WithProjection(string searchValue, FilterOperations operation, string culture)
    {
        // Arrange
        var oldCulture = CultureInfo.CurrentCulture;
        var newCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = newCulture;
        try
        {
            using TestDbContext context = Fixture.CreateContext();
            int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
            IQueryable<TestDto> baseQuery = context.TestEntities.Select(Mappings.SelectDto);
            List<TestDto> entities;
            if (operation == FilterOperations.Between)
            {
                string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
                if (!string.IsNullOrEmpty(values[0]))
                {
                    DateTime parsedValFrom = DateTime.Parse(values[0], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.DtVal >= parsedValFrom);
                }
                if (!string.IsNullOrEmpty(values[1]))
                {
                    DateTime parsedValTo = DateTime.Parse(values[1], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.DtVal <= parsedValTo);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    DateTime parsedVal = DateTime.Parse(searchValue, CultureInfo.CurrentCulture);
                    baseQuery = operation switch
                    {
                        FilterOperations.Equals => baseQuery.Where(x => x.DtVal == parsedVal),
                        FilterOperations.NotEqual => baseQuery.Where(x => x.DtVal != parsedVal),
                        FilterOperations.GreaterThan => baseQuery.Where(x => x.DtVal > parsedVal),
                        FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.DtVal >= parsedVal),
                        FilterOperations.LessThan => baseQuery.Where(x => x.DtVal < parsedVal),
                        FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.DtVal <= parsedVal),
                        _ => throw new InvalidOperationException($"{operation} not supported")
                    };
                }
            }
            entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
            IFormCollection form = TestFormBuilder.Create()
                                                  .AddColumn(nameof(TestDto.DtVal), new DateFilterModel { SearchValue = searchValue, FilterType = operation })
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
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidDateCases))]
    public async Task Filter_ValidNullableDateTime(string searchValue, FilterOperations operation, string culture)
    {
        // Arrange
        var oldCulture = CultureInfo.CurrentCulture;
        var newCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = newCulture;
        try
        {
            using TestDbContext context = Fixture.CreateContext();
            int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
            IQueryable<TestEntity> baseQuery = context.TestEntities.AsQueryable();
            List<TestEntity> entities;
            if (operation == FilterOperations.Between)
            {
                string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
                if (!string.IsNullOrEmpty(values[0]))
                {
                    DateTime parsedValFrom = DateTime.Parse(values[0], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.NullableDateTime >= parsedValFrom);
                }
                if (!string.IsNullOrEmpty(values[1]))
                {
                    DateTime parsedValTo = DateTime.Parse(values[1], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.NullableDateTime <= parsedValTo);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    DateTime parsedVal = DateTime.Parse(searchValue, CultureInfo.CurrentCulture);
                    baseQuery = operation switch
                    {
                        FilterOperations.Equals => baseQuery.Where(x => x.NullableDateTime == parsedVal),
                        FilterOperations.NotEqual => baseQuery.Where(x => x.NullableDateTime != parsedVal),
                        FilterOperations.GreaterThan => baseQuery.Where(x => x.NullableDateTime > parsedVal),
                        FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.NullableDateTime >= parsedVal),
                        FilterOperations.LessThan => baseQuery.Where(x => x.NullableDateTime < parsedVal),
                        FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.NullableDateTime <= parsedVal),
                        _ => throw new InvalidOperationException($"{operation} not supported")
                    };
                }
            }
            entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
            IFormCollection form = TestFormBuilder.Create()
                                                  .AddColumn(nameof(TestEntity.NullableDateTime), new DateFilterModel { SearchValue = searchValue, FilterType = operation })
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
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidDateCases))]
    public async Task Filter_ValidNullableDateTime_WithProjection(string searchValue, FilterOperations operation, string culture)
    {
        // Arrange
        var oldCulture = CultureInfo.CurrentCulture;
        var newCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = newCulture;
        try
        {
            using TestDbContext context = Fixture.CreateContext();
            int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
            IQueryable<TestDto> baseQuery = context.TestEntities.Select(Mappings.SelectDto);
            List<TestDto> entities;
            if (operation == FilterOperations.Between)
            {
                string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
                if (!string.IsNullOrEmpty(values[0]))
                {
                    DateTime parsedValFrom = DateTime.Parse(values[0], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.NullDt >= parsedValFrom);
                }
                if (!string.IsNullOrEmpty(values[1]))
                {
                    DateTime parsedValTo = DateTime.Parse(values[1], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.NullDt <= parsedValTo);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    DateTime parsedVal = DateTime.Parse(searchValue, CultureInfo.CurrentCulture);
                    baseQuery = operation switch
                    {
                        FilterOperations.Equals => baseQuery.Where(x => x.NullDt == parsedVal),
                        FilterOperations.NotEqual => baseQuery.Where(x => x.NullDt != parsedVal),
                        FilterOperations.GreaterThan => baseQuery.Where(x => x.NullDt > parsedVal),
                        FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.NullDt >= parsedVal),
                        FilterOperations.LessThan => baseQuery.Where(x => x.NullDt < parsedVal),
                        FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.NullDt <= parsedVal),
                        _ => throw new InvalidOperationException($"{operation} not supported")
                    };
                }
            }
            entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
            IFormCollection form = TestFormBuilder.Create()
                                                  .AddColumn(nameof(TestDto.NullDt), new DateFilterModel { SearchValue = searchValue, FilterType = operation })
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
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidDateCases))]
    public async Task Filter_ValidDateOnly(string searchValue, FilterOperations operation, string culture)
    {
        // Arrange
        var oldCulture = CultureInfo.CurrentCulture;
        var newCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = newCulture;
        try
        {
            using TestDbContext context = Fixture.CreateContext();
            int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
            IQueryable<TestEntity> baseQuery = context.TestEntities.AsQueryable();
            List<TestEntity> entities;
            if (operation == FilterOperations.Between)
            {
                string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
                if (!string.IsNullOrEmpty(values[0]))
                {
                    DateOnly parsedValFrom = DateOnly.Parse(values[0], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.DateOnlyVal >= parsedValFrom);
                }
                if (!string.IsNullOrEmpty(values[1]))
                {
                    DateOnly parsedValTo = DateOnly.Parse(values[1], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.DateOnlyVal <= parsedValTo);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    DateOnly parsedVal = DateOnly.Parse(searchValue, CultureInfo.CurrentCulture);
                    baseQuery = operation switch
                    {
                        FilterOperations.Equals => baseQuery.Where(x => x.DateOnlyVal == parsedVal),
                        FilterOperations.NotEqual => baseQuery.Where(x => x.DateOnlyVal != parsedVal),
                        FilterOperations.GreaterThan => baseQuery.Where(x => x.DateOnlyVal > parsedVal),
                        FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.DateOnlyVal >= parsedVal),
                        FilterOperations.LessThan => baseQuery.Where(x => x.DateOnlyVal < parsedVal),
                        FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.DateOnlyVal <= parsedVal),
                        _ => throw new InvalidOperationException($"{operation} not supported")
                    };
                }
            }
            entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
            IFormCollection form = TestFormBuilder.Create()
                                                  .AddColumn(nameof(TestEntity.DateOnlyVal), new DateFilterModel { SearchValue = searchValue, FilterType = operation })
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
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidDateCases))]
    public async Task Filter_ValidDateOnly_WithProjection(string searchValue, FilterOperations operation, string culture)
    {
        // Arrange
        var oldCulture = CultureInfo.CurrentCulture;
        var newCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = newCulture;
        try
        {
            using TestDbContext context = Fixture.CreateContext();
            int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
            IQueryable<TestDto> baseQuery = context.TestEntities.Select(Mappings.SelectDto);
            List<TestDto> entities;
            if (operation == FilterOperations.Between)
            {
                string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
                if (!string.IsNullOrEmpty(values[0]))
                {
                    DateOnly parsedValFrom = DateOnly.Parse(values[0], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.DoVal >= parsedValFrom);
                }
                if (!string.IsNullOrEmpty(values[1]))
                {
                    DateOnly parsedValTo = DateOnly.Parse(values[1], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.DoVal <= parsedValTo);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    DateOnly parsedVal = DateOnly.Parse(searchValue, CultureInfo.CurrentCulture);
                    baseQuery = operation switch
                    {
                        FilterOperations.Equals => baseQuery.Where(x => x.DoVal == parsedVal),
                        FilterOperations.NotEqual => baseQuery.Where(x => x.DoVal != parsedVal),
                        FilterOperations.GreaterThan => baseQuery.Where(x => x.DoVal > parsedVal),
                        FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.DoVal >= parsedVal),
                        FilterOperations.LessThan => baseQuery.Where(x => x.DoVal < parsedVal),
                        FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.DoVal <= parsedVal),
                        _ => throw new InvalidOperationException($"{operation} not supported")
                    };
                }
            }
            entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
            IFormCollection form = TestFormBuilder.Create()
                                                  .AddColumn(nameof(TestDto.DoVal), new DateFilterModel { SearchValue = searchValue, FilterType = operation })
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
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidDateCases))]
    public async Task Filter_ValidNullableDateOnly(string searchValue, FilterOperations operation, string culture)
    {
        // Arrange
        var oldCulture = CultureInfo.CurrentCulture;
        var newCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = newCulture;
        try
        {
            using TestDbContext context = Fixture.CreateContext();
            int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
            IQueryable<TestEntity> baseQuery = context.TestEntities.AsQueryable();
            List<TestEntity> entities;
            if (operation == FilterOperations.Between)
            {
                string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
                if (!string.IsNullOrEmpty(values[0]))
                {
                    DateOnly parsedValFrom = DateOnly.Parse(values[0], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.NullableDateOnly >= parsedValFrom);
                }
                if (!string.IsNullOrEmpty(values[1]))
                {
                    DateOnly parsedValTo = DateOnly.Parse(values[1], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.NullableDateOnly <= parsedValTo);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    DateOnly parsedVal = DateOnly.Parse(searchValue, CultureInfo.CurrentCulture);
                    baseQuery = operation switch
                    {
                        FilterOperations.Equals => baseQuery.Where(x => x.NullableDateOnly == parsedVal),
                        FilterOperations.NotEqual => baseQuery.Where(x => x.NullableDateOnly != parsedVal),
                        FilterOperations.GreaterThan => baseQuery.Where(x => x.NullableDateOnly > parsedVal),
                        FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.NullableDateOnly >= parsedVal),
                        FilterOperations.LessThan => baseQuery.Where(x => x.NullableDateOnly < parsedVal),
                        FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.NullableDateOnly <= parsedVal),
                        _ => throw new InvalidOperationException($"{operation} not supported")
                    };
                }
            }
            entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
            IFormCollection form = TestFormBuilder.Create()
                                                  .AddColumn(nameof(TestEntity.NullableDateOnly), new DateFilterModel { SearchValue = searchValue, FilterType = operation })
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
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidDateCases))]
    public async Task Filter_ValidNullableDateOnly_WithProjection(string searchValue, FilterOperations operation, string culture)
    {
        // Arrange
        var oldCulture = CultureInfo.CurrentCulture;
        var newCulture = new CultureInfo(culture);
        CultureInfo.CurrentCulture = newCulture;
        try
        {
            using TestDbContext context = Fixture.CreateContext();
            int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
            IQueryable<TestDto> baseQuery = context.TestEntities.Select(Mappings.SelectDto);
            List<TestDto> entities;
            if (operation == FilterOperations.Between)
            {
                string[] values = searchValue.Split(FilterParsingOptions.Default.BetweenSeparator);
                if (!string.IsNullOrEmpty(values[0]))
                {
                    DateOnly parsedValFrom = DateOnly.Parse(values[0], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.NullDo >= parsedValFrom);
                }
                if (!string.IsNullOrEmpty(values[1]))
                {
                    DateOnly parsedValTo = DateOnly.Parse(values[1], CultureInfo.CurrentCulture);
                    baseQuery = baseQuery.Where(x => x.NullDo <= parsedValTo);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(searchValue))
                {
                    DateOnly parsedVal = DateOnly.Parse(searchValue, CultureInfo.CurrentCulture);
                    baseQuery = operation switch
                    {
                        FilterOperations.Equals => baseQuery.Where(x => x.NullDo == parsedVal),
                        FilterOperations.NotEqual => baseQuery.Where(x => x.NullDo != parsedVal),
                        FilterOperations.GreaterThan => baseQuery.Where(x => x.NullDo > parsedVal),
                        FilterOperations.GreaterThanOrEqual => baseQuery.Where(x => x.NullDo >= parsedVal),
                        FilterOperations.LessThan => baseQuery.Where(x => x.NullDo < parsedVal),
                        FilterOperations.LessThanOrEqual => baseQuery.Where(x => x.NullDo <= parsedVal),
                        _ => throw new InvalidOperationException($"{operation} not supported")
                    };
                }
            }
            entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
            IFormCollection form = TestFormBuilder.Create()
                                                  .AddColumn(nameof(TestDto.NullDo), new DateFilterModel { SearchValue = searchValue, FilterType = operation })
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
        finally
        {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    [Theory, Trait("Category", "ColumnFilter")]
    [MemberData(nameof(ValidStringCases))]
    public async Task Filter_ValidString(string searchValue, FilterOperations operation)
    {
        // Arrange
        using TestDbContext context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        IQueryable<TestEntity> baseQuery = context.TestEntities.AsQueryable();
        List<TestEntity> entities;
        if (!string.IsNullOrEmpty(searchValue))
        {
            baseQuery = operation switch
            {
                FilterOperations.Equals => baseQuery.Where(x => x.StringVal == searchValue),
                FilterOperations.NotEqual => baseQuery.Where(x => x.StringVal != searchValue),
                FilterOperations.Contains => baseQuery.Where(x => x.StringVal.Contains(searchValue)),
                FilterOperations.DoesNotContain => baseQuery.Where(x => !x.StringVal.Contains(searchValue)),
                FilterOperations.StartsWith => baseQuery.Where(x => x.StringVal.StartsWith(searchValue)),
                FilterOperations.EndsWith => baseQuery.Where(x => x.StringVal.EndsWith(searchValue)),
                _ => throw new InvalidOperationException($"{operation} not supported")
            };
        }
        entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
        IFormCollection form = TestFormBuilder.Create()
                                              .AddColumn(nameof(TestEntity.StringVal), new TextFilterModel { SearchValue = searchValue, FilterType = operation })
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
    [MemberData(nameof(ValidStringCases))]
    public async Task Filter_ValidString_WithProjection(string searchValue, FilterOperations operation)
    {
        // Arrange
        using TestDbContext context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        IQueryable<TestDto> baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        List<TestDto> entities;
        if (!string.IsNullOrEmpty(searchValue))
        {
            baseQuery = operation switch
            {
                FilterOperations.Equals => baseQuery.Where(x => x.StrVal == searchValue),
                FilterOperations.NotEqual => baseQuery.Where(x => x.StrVal != searchValue),
                FilterOperations.Contains => baseQuery.Where(x => x.StrVal.Contains(searchValue)),
                FilterOperations.DoesNotContain => baseQuery.Where(x => !x.StrVal.Contains(searchValue)),
                FilterOperations.StartsWith => baseQuery.Where(x => x.StrVal.StartsWith(searchValue)),
                FilterOperations.EndsWith => baseQuery.Where(x => x.StrVal.EndsWith(searchValue)),
                _ => throw new InvalidOperationException($"{operation} not supported")
            };
        }
        entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
        IFormCollection form = TestFormBuilder.Create()
                                              .AddColumn(nameof(TestDto.StrVal), new TextFilterModel { SearchValue = searchValue, FilterType = operation })
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
    [MemberData(nameof(ValidStringCases))]
    public async Task Filter_ValidNullableString(string searchValue, FilterOperations operation)
    {
        // Arrange
        using TestDbContext context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        IQueryable<TestEntity> baseQuery = context.TestEntities.AsQueryable();
        List<TestEntity> entities;
        if (!string.IsNullOrEmpty(searchValue))
        {
            baseQuery = operation switch
            {
                FilterOperations.Equals => baseQuery.Where(x => x.NullableString != null && x.NullableString == searchValue),
                FilterOperations.NotEqual => baseQuery.Where(x => x.NullableString == null || x.NullableString != searchValue),
                FilterOperations.Contains => baseQuery.Where(x => x.NullableString != null && x.NullableString.Contains(searchValue)),
                FilterOperations.DoesNotContain => baseQuery.Where(x => x.NullableString == null || !x.NullableString.Contains(searchValue)),
                FilterOperations.StartsWith => baseQuery.Where(x => x.NullableString != null && x.NullableString.StartsWith(searchValue)),
                FilterOperations.EndsWith => baseQuery.Where(x => x.NullableString != null && x.NullableString.EndsWith(searchValue)),
                _ => throw new InvalidOperationException($"{operation} not supported")
            };
        }
        entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
        IFormCollection form = TestFormBuilder.Create()
                                              .AddColumn(nameof(TestEntity.NullableString), new TextFilterModel { SearchValue = searchValue, FilterType = operation })
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
    [MemberData(nameof(ValidStringCases))]
    public async Task Filter_ValidNullableString_WithProjection(string searchValue, FilterOperations operation)
    {
        // Arrange
        using TestDbContext context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        IQueryable<TestDto> baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        List<TestDto> entities;
        if (!string.IsNullOrEmpty(searchValue))
        {
            baseQuery = operation switch
            {
                FilterOperations.Equals => baseQuery.Where(x => x.NullStr != null && x.NullStr == searchValue),
                FilterOperations.NotEqual => baseQuery.Where(x => x.NullStr == null || x.NullStr != searchValue),
                FilterOperations.Contains => baseQuery.Where(x => x.NullStr != null && x.NullStr.Contains(searchValue)),
                FilterOperations.DoesNotContain => baseQuery.Where(x => x.NullStr == null || !x.NullStr.Contains(searchValue)),
                FilterOperations.StartsWith => baseQuery.Where(x => x.NullStr != null && x.NullStr.StartsWith(searchValue)),
                FilterOperations.EndsWith => baseQuery.Where(x => x.NullStr != null && x.NullStr.EndsWith(searchValue)),
                _ => throw new InvalidOperationException($"{operation} not supported")
            };
        }
        entities = await baseQuery.ToListAsync(TestContext.Current.CancellationToken);
        IFormCollection form = TestFormBuilder.Create()
                                              .AddColumn(nameof(TestDto.NullStr), new TextFilterModel { SearchValue = searchValue, FilterType = operation })
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

