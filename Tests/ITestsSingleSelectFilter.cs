using DataTables.ServerSideProcessing.EFCore;
using Microsoft.EntityFrameworkCore;
using Tests.Data;
using Tests.Fixtures;

namespace Tests;

public interface ITestsSingleSelectFilter<TFixture> where TFixture : ITestDbFixture
{
    TFixture Fixture { get; }

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_IntVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.IntVal.ToString() == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.IntVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_IntVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.IntVal.ToString() == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.IntVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullableInt(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.NullableInt != null && e.NullableInt.ToString() == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableInt), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullInt_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullInt != null && e.NullInt.ToString() == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullInt), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_DecimalVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.DecimalVal.ToString() == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.DecimalVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_DecVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.DecVal.ToString() == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.DecVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullableDecimal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.NullableDecimal != null && e.NullableDecimal.ToString() == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableDecimal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullDec_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullDec != null && e.NullDec.ToString() == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullDec), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_DateTimeVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.DateTimeVal.ToString() == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.DateTimeVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_DtVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.DtVal.ToString() == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.DtVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullableDateTime(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.NullableDateTime != null && e.NullableDateTime.ToString() == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableDateTime), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullDt_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullDt != null && e.NullDt.ToString() == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullDt), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_DateOnlyVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.DateOnlyVal.ToString() == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.DateOnlyVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_DoVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.DoVal.ToString() == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.DoVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullableDateOnly(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.NullableDateOnly != null && e.NullableDateOnly.ToString() == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableDateOnly), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullDo_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullDo != null && e.NullDo.ToString() == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullDo), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_EnumVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.EnumVal.ToString() == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.EnumVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_EnumVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.EnumVal.ToString() == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.EnumVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullableEnum(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.NullableEnum != null && e.NullableEnum.ToString() == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableEnum), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullableEnum_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                           .Where(e => e.NullableEnum != null && e.NullableEnum.ToString() == searchValue)
                                           .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullableEnum), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_BoolVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.BoolVal.ToString() == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.BoolVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_BoolVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.BoolVal.ToString() == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.BoolVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullableBool(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.NullableBool != null && e.NullableBool.ToString() == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableBool), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullableBool_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullableBool != null && e.NullableBool.ToString() == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullableBool), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_StringVal(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.StringVal == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.StringVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_StrVal_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.StrVal == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.StrVal), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullableString(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(e => e.NullableString != null && e.NullableString == searchValue).ToList();
        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestEntity.NullableString), new SingleSelectFilterModel { SearchValue = searchValue })
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

    [Theory, Trait("Category", "SingleSelect")]
    [InlineData("123")]
    [InlineData("999")]
    [InlineData("test")]
    public async Task Select_NullStr_WithProjection(string searchValue)
    {
        // Arrange
        using var context = Fixture.CreateContext();
        int recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                              .Where(e => e.NullStr != null && e.NullStr == searchValue)
                                              .ToList();

        var form = TestFormBuilder.Create()
                                  .AddColumn(nameof(TestDto.NullStr), new SingleSelectFilterModel { SearchValue = searchValue })
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

