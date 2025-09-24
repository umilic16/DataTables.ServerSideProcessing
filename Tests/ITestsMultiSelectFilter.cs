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
            new($"999{sep}123{sep}test123"),
            new($"test{sep}0{sep}testing")
        ];
    }

    [Theory, Trait("Category", "MultiSelect")]
    [MemberData(nameof(TestData))]
    public async Task Select_IntVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => searchValues.Contains(e.IntVal.ToString())).ToList();
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
    public async Task Select_IntVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => searchValues.Contains(e.IntVal.ToString()))
                                              .ToList();

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
    public async Task Select_NullableInt(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => e.NullableInt != null && searchValues.Contains(e.NullableInt.ToString())).ToList();
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
    public async Task Select_NullInt_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullInt != null && searchValues.Contains(e.NullInt.ToString()))
                                              .ToList();

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
    public async Task Select_DecimalVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => searchValues.Contains(e.DecimalVal.ToString())).ToList();
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
    public async Task Select_DecVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => searchValues.Contains(e.DecVal.ToString()))
                                              .ToList();

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
    public async Task Select_NullableDecimal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => e.NullableDecimal != null && searchValues.Contains(e.NullableDecimal.ToString())).ToList();
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
    public async Task Select_NullDec_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullDec != null && searchValues.Contains(e.NullDec.ToString()))
                                              .ToList();

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
    [MemberData(nameof(TestData))]
    public async Task Select_DateTimeVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => searchValues.Contains(e.DateTimeVal.ToString())).ToList();
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
    [MemberData(nameof(TestData))]
    public async Task Select_DtVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => searchValues.Contains(e.DtVal.ToString()))
                                              .ToList();

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
    [MemberData(nameof(TestData))]
    public async Task Select_NullableDateTime(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => e.NullableDateTime != null && searchValues.Contains(e.NullableDateTime.ToString())).ToList();
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
    [MemberData(nameof(TestData))]
    public async Task Select_NullDt_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullDt != null && searchValues.Contains(e.NullDt.ToString()))
                                              .ToList();

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
    [MemberData(nameof(TestData))]
    public async Task Select_DateOnlyVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => searchValues.Contains(e.DateOnlyVal.ToString())).ToList();
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
    [MemberData(nameof(TestData))]
    public async Task Select_DoVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => searchValues.Contains(e.DoVal.ToString()))
                                              .ToList();

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
    [MemberData(nameof(TestData))]
    public async Task Select_NullableDateOnly(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => e.NullableDateOnly != null && searchValues.Contains(e.NullableDateOnly.ToString())).ToList();
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
    [MemberData(nameof(TestData))]
    public async Task Select_NullDo_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullDo != null && searchValues.Contains(e.NullDo.ToString()))
                                              .ToList();

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
    [MemberData(nameof(TestData))]
    public async Task Select_EnumVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => searchValues.Contains(e.EnumVal.ToString())).ToList();
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
    [MemberData(nameof(TestData))]
    public async Task Select_EnumVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => searchValues.Contains(e.EnumVal.ToString()))
                                              .ToList();

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
    [MemberData(nameof(TestData))]
    public async Task Select_NullableEnum(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => e.NullableEnum != null && searchValues.Contains(e.NullableEnum.ToString())).ToList();
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
    [MemberData(nameof(TestData))]
    public async Task Select_NullableEnum_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullableEnum != null && searchValues.Contains(e.NullableEnum.ToString()))
                                              .ToList();

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
    public async Task Select_BoolVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => searchValues.Contains(e.BoolVal.ToString())).ToList();
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
    public async Task Select_BoolVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => searchValues.Contains(e.BoolVal.ToString()))
                                              .ToList();

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
    public async Task Select_NullableBool(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => e.NullableBool != null && searchValues.Contains(e.NullableBool.ToString())).ToList();
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
    public async Task Select_NullableBool_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullableBool != null && searchValues.Contains(e.NullableBool.ToString()))
                                              .ToList();

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
    public async Task Select_StringVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => searchValues.Contains(e.StringVal)).ToList();
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
    public async Task Select_StrVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                           .Where(e => searchValues.Contains(e.StrVal))
                                           .ToList();

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
    public async Task Select_NullableString(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Where(e => e.NullableString != null && searchValues.Contains(e.NullableString)).ToList();
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
    public async Task Select_NullStr_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        string[] searchValues = searchValue.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullStr != null && searchValues.Contains(e.NullStr))
                                              .ToList();

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
}

