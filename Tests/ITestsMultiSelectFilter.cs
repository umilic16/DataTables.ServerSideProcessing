using System.Globalization;
using DataTables.ServerSideProcessing.Data.Models;
using DataTables.ServerSideProcessing.EFCore;
using Microsoft.EntityFrameworkCore;
using Tests.Data;
using Tests.Fixtures;

namespace Tests;

public interface ITestsMultiSelectFilter<TFixture> where TFixture : ITestDbFixture
{
    TFixture Fixture { get; }
    private static string sep => FilterParsingOptions.Default.MultiSelectSeparator;
    public static List<TheoryDataRow<string>> TestData()
    {
        return
        [
            new("123"),
            new($"999{sep}true{sep}test123{sep}1,23"),
            new($"test{sep}0{sep}-4,5{sep}false")
        ];
    }
    public static List<TheoryDataRow<string>> TestDataEnums()
    {
        return
        [
            new("1"),
            new($"2{sep}123{sep}{Something.Alpha}"),
            new($"{Something.Alpha}{sep}{Something.Gamma}{sep}testing")
        ];
    }
    public static List<TheoryDataRow<string>> TestDataDates()
    {
        return
        [
            new("123"),
            new($"22.01.2025{sep}28.09.2030{sep}31.03.2010"),
            new($"17.04.2017{sep}0{sep}13.02.2022")
        ];
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_IntVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => int.TryParse(s, CultureInfo.CurrentCulture, out var parsedInt) ? parsedInt : (int?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.IntVal));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.IntVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.IntVal), response.Data.Select(x => x.IntVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_IntVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => int.TryParse(s, CultureInfo.CurrentCulture, out var parsedInt) ? parsedInt : (int?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.IntVal));
        }
        var entities = baseQuery.ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.IntVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.IntVal), response.Data.Select(x => x.IntVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_NullableInt(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => int.TryParse(s, CultureInfo.CurrentCulture, out var parsedInt) ? parsedInt : (int?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullableInt != null && parsedSearch.Contains(e.NullableInt.Value));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableInt), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullableInt), response.Data.Select(x => x.NullableInt));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_NullInt_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => int.TryParse(s, CultureInfo.CurrentCulture, out var parsedInt) ? parsedInt : (int?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullInt != null && parsedSearch.Contains(e.NullInt.Value));
        }
        var entities = baseQuery.ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullInt), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullInt), response.Data.Select(x => x.NullInt));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_DecimalVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => decimal.TryParse(s, CultureInfo.CurrentCulture, out var parsedInt) ? parsedInt : (decimal?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.DecimalVal));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.DecimalVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.DecimalVal), response.Data.Select(x => x.DecimalVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_DecVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => decimal.TryParse(s, CultureInfo.CurrentCulture, out var parsedInt) ? parsedInt : (decimal?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.DecVal));
        }
        var entities = baseQuery.ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.DecVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.DecVal), response.Data.Select(x => x.DecVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_NullableDecimal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => decimal.TryParse(s, CultureInfo.CurrentCulture, out var parsedInt) ? parsedInt : (decimal?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullableDecimal != null && parsedSearch.Contains(e.NullableDecimal.Value));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableDecimal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullableDecimal), response.Data.Select(x => x.NullableDecimal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_NullDec_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => decimal.TryParse(s, CultureInfo.CurrentCulture, out var parsedInt) ? parsedInt : (decimal?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullDec != null && parsedSearch.Contains(e.NullDec.Value));
        }
        var entities = baseQuery.ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullDec), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullDec), response.Data.Select(x => x.NullDec));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataDates))]
    public async Task MultiSelect_DateTimeVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        CultureInfo.CurrentCulture = new CultureInfo("sr");
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d) ? d : (DateTime?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.DateTimeVal));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.DateTimeVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.DateTimeVal), response.Data.Select(x => x.DateTimeVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataDates))]
    public async Task MultiSelect_DtVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        CultureInfo.CurrentCulture = new CultureInfo("sr");
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d) ? d : (DateTime?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.DtVal));
        }
        var entities = baseQuery.ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.DtVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.DtVal), response.Data.Select(x => x.DtVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataDates))]
    public async Task MultiSelect_NullableDateTime(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        CultureInfo.CurrentCulture = new CultureInfo("sr");
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d) ? d : (DateTime?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullableDateTime != null && parsedSearch.Contains(e.NullableDateTime.Value));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableDateTime), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullableDateTime), response.Data.Select(x => x.NullableDateTime));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataDates))]
    public async Task MultiSelect_NullDt_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        CultureInfo.CurrentCulture = new CultureInfo("sr");
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d) ? d : (DateTime?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullDt != null && parsedSearch.Contains(e.NullDt.Value));
        }
        var entities = baseQuery.ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullDt), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullDt), response.Data.Select(x => x.NullDt));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataDates))]
    public async Task MultiSelect_DateOnlyVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        CultureInfo.CurrentCulture = new CultureInfo("sr");
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => DateOnly.TryParse(s, CultureInfo.CurrentCulture, out var d) ? d : (DateOnly?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.DateOnlyVal));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.DateOnlyVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.DateOnlyVal), response.Data.Select(x => x.DateOnlyVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataDates))]
    public async Task MultiSelect_DoVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => DateOnly.TryParse(s, CultureInfo.CurrentCulture, out var d) ? d : (DateOnly?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.DoVal));
        }
        var entities = baseQuery.ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.DoVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.DoVal), response.Data.Select(x => x.DoVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataDates))]
    public async Task MultiSelect_NullableDateOnly(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => DateOnly.TryParse(s, CultureInfo.CurrentCulture, out var d) ? d : (DateOnly?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullableDateOnly != null && parsedSearch.Contains(e.NullableDateOnly.Value));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableDateOnly), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullableDateOnly), response.Data.Select(x => x.NullableDateOnly));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataDates))]
    public async Task MultiSelect_NullDo_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => DateOnly.TryParse(s, CultureInfo.CurrentCulture, out var d) ? d : (DateOnly?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullDo != null && parsedSearch.Contains(e.NullDo.Value));
        }
        var entities = baseQuery.ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullDo), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullDo), response.Data.Select(x => x.NullDo));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataEnums))]
    public async Task MultiSelect_EnumVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => Enum.TryParse<Something>(s, out var parsedEnum) ? parsedEnum : (Something?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.EnumVal));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.EnumVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.EnumVal), response.Data.Select(x => x.EnumVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataEnums))]
    public async Task MultiSelect_EnumVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => Enum.TryParse<Something>(s, out var parsedEnum) ? parsedEnum : (Something?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.EnumVal));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.EnumVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.EnumVal), response.Data.Select(x => x.EnumVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataEnums))]
    public async Task MultiSelect_NullableEnum(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => Enum.TryParse<Something>(s, out var parsedEnum) ? parsedEnum : (Something?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullableEnum != null && parsedSearch.Contains(e.NullableEnum.Value));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableEnum), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullableEnum), response.Data.Select(x => x.NullableEnum));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataEnums))]
    public async Task MultiSelect_NullableEnum_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => Enum.TryParse<Something>(s, out var parsedEnum) ? parsedEnum : (Something?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullableEnum != null && parsedSearch.Contains(e.NullableEnum.Value));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullableEnum), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullableEnum), response.Data.Select(x => x.NullableEnum));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_BoolVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => bool.TryParse(s, out var parsedBool) ? parsedBool : (bool?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.BoolVal));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.BoolVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.BoolVal), response.Data.Select(x => x.BoolVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_BoolVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => bool.TryParse(s, out var parsedBool) ? parsedBool : (bool?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.BoolVal));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.BoolVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.BoolVal), response.Data.Select(x => x.BoolVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_NullableBool(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                              .Select(s => bool.TryParse(s, out var parsedBool) ? parsedBool : (bool?)null)
                              .Where(x => x != null)
                              .Select(x => x!.Value)
                              .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullableBool != null && parsedSearch.Contains(e.NullableBool.Value));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableBool), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullableBool), response.Data.Select(x => x.NullableBool));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_NullableBool_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => bool.TryParse(s, out var parsedBool) ? parsedBool : (bool?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullableBool != null && parsedSearch.Contains(e.NullableBool.Value));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullableBool), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullableBool), response.Data.Select(x => x.NullableBool));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_StringVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries).ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.StringVal));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.StringVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.StringVal), response.Data.Select(x => x.StringVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_StrVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries).ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.StrVal));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.StrVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.StrVal), response.Data.Select(x => x.StrVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_NullableString(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                              .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullableString != null && parsedSearch.Contains(e.NullableString));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableString), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullableString), response.Data.Select(x => x.NullableString));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task MultiSelect_NullStr_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries).ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullStr != null && parsedSearch.Contains(e.NullStr));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullStr), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullStr), response.Data.Select(x => x.NullStr));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataDates))]
    public async Task MultiSelect_DateTimeOffsetVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        CultureInfo.CurrentCulture = new CultureInfo("sr");
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => DateTimeOffset.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d) ? d : (DateTimeOffset?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.DateTimeOffsetVal));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.DateTimeOffsetVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.DateTimeOffsetVal), response.Data.Select(x => x.DateTimeOffsetVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataDates))]
    public async Task MultiSelect_DtoVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => DateTimeOffset.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d) ? d : (DateTimeOffset?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => parsedSearch.Contains(e.DtoVal));
        }
        var entities = baseQuery.ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.DtoVal), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.DtoVal), response.Data.Select(x => x.DtoVal));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataDates))]
    public async Task MultiSelect_NullableDateTimeOffset(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => DateTimeOffset.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d) ? d : (DateTimeOffset?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.AsQueryable();
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullableDateTimeOffset != null && parsedSearch.Contains(e.NullableDateTimeOffset.Value));
        }
        var entities = baseQuery.ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableDateTimeOffset), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullableDateTimeOffset), response.Data.Select(x => x.NullableDateTimeOffset));
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestDataDates))]
    public async Task MultiSelect_NullDto_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var parsedSearch = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => DateTimeOffset.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d) ? d : (DateTimeOffset?)null)
                                      .Where(x => x != null)
                                      .Select(x => x!.Value)
                                      .ToArray();
        var baseQuery = context.TestEntities.Select(Mappings.SelectDto);
        if (parsedSearch.Length > 0)
        {
            baseQuery = baseQuery.Where(e => e.NullDto != null && parsedSearch.Contains(e.NullDto.Value));
        }
        var entities = baseQuery.ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullDto), new MultiSelectFilterModel { SearchValue = searchValue })
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                    .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, response.RecordsTotal);
        Assert.Equal(entities.Count, response.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.NullDto), response.Data.Select(x => x.NullDto));
    }
}

