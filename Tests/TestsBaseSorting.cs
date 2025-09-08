using DataTables.ServerSideProcessing.Data.Enums;
using DataTables.ServerSideProcessing.EFCore;
using Tests.Data;
using Tests.Fixtures;

namespace Tests;
public abstract partial class TestsBase<TFixture>(TFixture fixture) where TFixture : ITestDbFixture
{
    protected readonly TFixture Fixture = fixture;

    [Fact]
    public async Task Sort_IntVal_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.IntVal).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.IntVal), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.IntVal), response.Data.Select(x => x.IntVal));
    }

    [Fact]
    public async Task Sort_IntVal_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.IntVal)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.IntVal), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.IntVal), response.Data.Select(x => x.IntVal));
    }

    [Fact]
    public async Task Sort_NullableInt_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.NullableInt).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableInt), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullableInt), response.Data.Select(x => x.NullableInt));
    }

    [Fact]
    public async Task Sort_NullInt_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.NullInt)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullInt), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullInt), response.Data.Select(x => x.NullInt));
    }

    [Fact]
    public async Task Sort_DecimalVal_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.DecimalVal).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.DecimalVal), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.DecimalVal), response.Data.Select(x => x.DecimalVal));
    }

    [Fact]
    public async Task Sort_DecVal_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.DecVal)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.DecVal), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.DecVal), response.Data.Select(x => x.DecVal));
    }

    [Fact]
    public async Task Sort_NullableDecimal_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.NullableDecimal).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableDecimal), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullableDecimal), response.Data.Select(x => x.NullableDecimal));
    }

    [Fact]
    public async Task Sort_NullDec_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.NullDec)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullDec), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullDec), response.Data.Select(x => x.NullDec));
    }

    [Fact]
    public async Task Sort_DateTimeVal_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.DateTimeVal).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.DateTimeVal), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.DateTimeVal), response.Data.Select(x => x.DateTimeVal));
    }

    [Fact]
    public async Task Sort_DtVal_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.DtVal)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.DtVal), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.DtVal), response.Data.Select(x => x.DtVal));
    }

    [Fact]
    public async Task Sort_NullableDateTime_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.NullableDateTime).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableDateTime), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullableDateTime), response.Data.Select(x => x.NullableDateTime));
    }

    [Fact]
    public async Task Sort_NullDt_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.NullDt)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullDt), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullDt), response.Data.Select(x => x.NullDt));
    }

    [Fact]
    public async Task Sort_DateOnlyVal_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.DateOnlyVal).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.DateOnlyVal), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.DateOnlyVal), response.Data.Select(x => x.DateOnlyVal));
    }

    [Fact]
    public async Task Sort_DoVal_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.DoVal)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.DoVal), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.DoVal), response.Data.Select(x => x.DoVal));
    }

    [Fact]
    public async Task Sort_NullableDateOnly_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.NullableDateOnly).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableDateOnly), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullableDateOnly), response.Data.Select(x => x.NullableDateOnly));
    }

    [Fact]
    public async Task Sort_NullDo_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.NullDo)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullDo), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullDo), response.Data.Select(x => x.NullDo));
    }

    [Fact]
    public async Task Sort_EnumVal_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.EnumVal).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.EnumVal), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.EnumVal), response.Data.Select(x => x.EnumVal));
    }

    [Fact]
    public async Task Sort_EnumVal_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.EnumVal)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.EnumVal), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.EnumVal), response.Data.Select(x => x.EnumVal));
    }

    [Fact]
    public async Task Sort_NullableEnumAscending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.NullableEnum).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableEnum), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullableEnum), response.Data.Select(x => x.NullableEnum));
    }

    [Fact]
    public async Task Sort_NullableEnum_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.NullableEnum)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullableEnum), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullableEnum), response.Data.Select(x => x.NullableEnum));
    }

    [Fact]
    public async Task Sort_BoolVal_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.BoolVal).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.BoolVal), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.BoolVal), response.Data.Select(x => x.BoolVal));
    }

    [Fact]
    public async Task Sort_BoolVal_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.BoolVal)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.BoolVal), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.BoolVal), response.Data.Select(x => x.BoolVal));
    }

    [Fact]
    public async Task Sort_NullableBool_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.NullableBool).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableBool), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullableBool), response.Data.Select(x => x.NullableBool));
    }

    [Fact]
    public async Task Sort_NullableBool_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.NullableBool)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullableBool), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullableBool), response.Data.Select(x => x.NullableBool));
    }

    [Fact]
    public async Task Sort_StringVal_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.StringVal).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.StringVal), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.StringVal), response.Data.Select(x => x.StringVal));
    }

    [Fact]
    public async Task Sort_StrVal_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.StrVal)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.StrVal), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.StrVal), response.Data.Select(x => x.StrVal));
    }

    [Fact]
    public async Task Sort_NullableString_Ascending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.OrderBy(e => e.NullableString).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableString), sortDirection: SortDirection.Ascending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullableString), response.Data.Select(x => x.NullableString));
    }

    [Fact]
    public async Task Sort_NullStr_WithProjection_Descending()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var allEntities = context.TestEntities.Select(Mappings.SelectDto)
                                              .OrderByDescending(e => e.NullStr)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullStr), sortDirection: SortDirection.Descending)
                                  .Build();

        using var contextNew = Fixture.CreateContext();

        // Act
        var response = await contextNew.TestEntities.ForDataTable(form)
                                                    .WithProjection(Mappings.SelectDto)
                                                    .BuildAsync();

        // Assert
        Assert.Equal(allEntities.Count, response.RecordsTotal);
        Assert.Equal(allEntities.Count, response.RecordsFiltered);
        Assert.Equal(allEntities.Select(x => x.NullStr), response.Data.Select(x => x.NullStr));
    }
}

