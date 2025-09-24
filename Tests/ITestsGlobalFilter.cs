using DataTables.ServerSideProcessing.EFCore;
using Microsoft.EntityFrameworkCore;
using Tests.Data;
using Tests.Fixtures;

namespace Tests;

public interface ITestsGlobalFilter<TFixture> where TFixture : ITestDbFixture
{
    TFixture Fixture { get; }

    [Fact, Trait("Category", "GlobalFilter")]
    public async Task GlobalFilter_On_NumericColumns()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var searchValue = "123";
        var recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(x => x.IntVal.ToString().Contains(searchValue)
                                                       || (x.NullableInt == null ? string.Empty : x.NullableInt.ToString())!.Contains(searchValue)
                                                       || x.DecimalVal.ToString().Contains(searchValue)
                                                       || (x.NullableDecimal == null ? string.Empty : x.NullableDecimal.ToString())!.Contains(searchValue))
                                           .ToList();

        var form = TestFormBuilder.Create()
                                  .WithGlobalSearch(searchValue)
                                  .Build();

        using var contextNew = Fixture.CreateContext();
        // Act
        var result = await contextNew.TestEntities.ForDataTable(form)
                                                  .WithGlobalFilter(nameof(TestEntity.IntVal), nameof(TestEntity.NullableInt), nameof(TestEntity.DecimalVal), nameof(TestEntity.NullableDecimal))
                                                  .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }

    [Fact, Trait("Category", "GlobalFilter")]
    public async Task GlobalFilter_On_NumericColumns_WithProjection()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var searchValue = "123";
        var recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                           .Where(x => x.IntVal.ToString().Contains(searchValue)
                                                       || (x.NullInt == null ? string.Empty : x.NullInt.ToString())!.Contains(searchValue)
                                                       || x.DecVal.ToString().Contains(searchValue)
                                                       || (x.NullDec == null ? string.Empty : x.NullDec.ToString())!.Contains(searchValue))
                                           .ToList();

        var form = TestFormBuilder.Create()
                                  .WithGlobalSearch(searchValue)
                                  .Build();

        using var contextNew = Fixture.CreateContext();
        // Act
        var result = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                  .WithGlobalFilter(nameof(TestDto.IntVal), nameof(TestDto.NullInt), nameof(TestDto.DecVal), nameof(TestDto.NullDec))
                                                  .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }

    [Fact, Trait("Category", "GlobalFilter")]
    public async Task GlobalFilter_On_DateColumns()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var searchValue = DateTime.Now.AddYears(5).Date.ToString();
        var recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(x => x.DateTimeVal.ToString().Contains(searchValue)
                                                       || (x.NullableDateTime == null ? string.Empty : x.NullableDateTime.ToString())!.Contains(searchValue)
                                                       || x.DateOnlyVal.ToString().Contains(searchValue)
                                                       || (x.NullableDateOnly == null ? string.Empty : x.NullableDateOnly.ToString())!.Contains(searchValue))
                                           .ToList();

        var form = TestFormBuilder.Create()
                                  .WithGlobalSearch(searchValue)
                                  .Build();

        using var contextNew = Fixture.CreateContext();
        // Act
        var result = await contextNew.TestEntities.ForDataTable(form)
                                                  .WithGlobalFilter(nameof(TestEntity.DateTimeVal), nameof(TestEntity.NullableDateTime), nameof(TestEntity.DateOnlyVal), nameof(TestEntity.NullableDateOnly))
                                                  .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }

    [Fact, Trait("Category", "GlobalFilter")]
    public async Task GlobalFilter_On_DateColumns_WithProjection()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var searchValue = DateTime.Now.AddYears(5).Date.ToString();
        var recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                           .Where(x => x.DtVal.ToString().Contains(searchValue)
                                                       || (x.NullDt == null ? string.Empty : x.NullDt.ToString())!.Contains(searchValue)
                                                       || x.DoVal.ToString().Contains(searchValue)
                                                       || (x.NullDo == null ? string.Empty : x.NullDo.ToString())!.Contains(searchValue))
                                           .ToList();

        var form = TestFormBuilder.Create()
                                  .WithGlobalSearch(searchValue)
                                  .Build();

        using var contextNew = Fixture.CreateContext();
        // Act
        var result = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                  .WithGlobalFilter(nameof(TestDto.DtVal), nameof(TestDto.NullDt), nameof(TestDto.DoVal), nameof(TestDto.NullDo))
                                                  .BuildAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }

    [Fact, Trait("Category", "GlobalFilter")]
    public async Task GlobalFilter_On_EnumColumns()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var searchValue = Something.Alpha.ToString();
        var recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(x => x.EnumVal.ToString().Contains(searchValue)
                                                       || (x.NullableEnum == null ? string.Empty : x.NullableEnum.ToString())!.Contains(searchValue))
                                           .ToList();

        var form = TestFormBuilder.Create()
                                  .WithGlobalSearch(searchValue)
                                  .Build();

        using var contextNew = Fixture.CreateContext();
        // Act
        var result = await contextNew.TestEntities.ForDataTable(form)
                                                  .WithGlobalFilter(nameof(TestEntity.EnumVal), nameof(TestEntity.NullableEnum))
                                                  .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }

    [Fact, Trait("Category", "GlobalFilter")]
    public async Task GlobalFilter_On_EnumColumns_WithProjection()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var searchValue = "1";
        var recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                           .Where(x => x.EnumVal.ToString().Contains(searchValue)
                                                       || (x.NullableEnum == null ? string.Empty : x.NullableEnum.ToString())!.Contains(searchValue))
                                           .ToList();

        var form = TestFormBuilder.Create()
                                  .WithGlobalSearch(searchValue)
                                  .Build();

        using var contextNew = Fixture.CreateContext();
        // Act
        var result = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                  .WithGlobalFilter(nameof(TestDto.EnumVal), nameof(TestDto.NullableEnum))
                                                  .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }

    [Fact, Trait("Category", "GlobalFilter")]
    public async Task GlobalFilter_On_BoolColumns()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var searchValue = "123";
        var recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(x => x.BoolVal.ToString().Contains(searchValue)
                                                       || (x.NullableBool == null ? string.Empty : x.NullableBool.ToString())!.Contains(searchValue))
                                           .ToList();

        var form = TestFormBuilder.Create()
                                  .WithGlobalSearch(searchValue)
                                  .Build();

        using var contextNew = Fixture.CreateContext();
        // Act
        var result = await contextNew.TestEntities.ForDataTable(form)
                                                  .WithGlobalFilter(nameof(TestEntity.BoolVal), nameof(TestEntity.NullableBool))
                                                  .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }

    [Fact, Trait("Category", "GlobalFilter")]
    public async Task GlobalFilter_On_BoolColumns_WithProjection()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var searchValue = "123";
        var recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                           .Where(x => x.BoolVal.ToString().Contains(searchValue)
                                                       || (x.NullableBool == null ? string.Empty : x.NullableBool.ToString())!.Contains(searchValue))
                                           .ToList();

        var form = TestFormBuilder.Create()
                                  .WithGlobalSearch(searchValue)
                                  .Build();

        using var contextNew = Fixture.CreateContext();
        // Act
        var result = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                  .WithGlobalFilter(nameof(TestDto.BoolVal), nameof(TestDto.NullableBool))
                                                  .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }

    [Fact, Trait("Category", "GlobalFilter")]
    public async Task GlobalFilter_On_StringColumns()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var searchValue = "test";
        var recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Where(x => x.StringVal.Contains(searchValue)
                                                       || (x.NullableString ?? string.Empty)!.Contains(searchValue))
                                           .ToList();

        var form = TestFormBuilder.Create()
                                  .WithGlobalSearch(searchValue)
                                  .Build();

        using var contextNew = Fixture.CreateContext();
        // Act
        var result = await contextNew.TestEntities.ForDataTable(form)
                                                  .WithGlobalFilter(nameof(TestEntity.StringVal), nameof(TestEntity.NullableString))
                                                  .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }

    [Fact, Trait("Category", "GlobalFilter")]
    public async Task GlobalFilter_On_StringColumns_WithProjection()
    {
        // Arrange
        using var context = Fixture.CreateContext();
        var searchValue = "~!@#$";
        var recordsTotal = await context.TestEntities.CountAsync(TestContext.Current.CancellationToken);
        var entities = context.TestEntities.Select(Mappings.SelectDto)
                                           .Where(x => x.StrVal.Contains(searchValue)
                                                       || (x.NullStr ?? string.Empty)!.Contains(searchValue))
                                           .ToList();

        var form = TestFormBuilder.Create()
                                  .WithGlobalSearch(searchValue)
                                  .Build();

        using var contextNew = Fixture.CreateContext();
        // Act
        var result = await contextNew.TestEntities.ForDataTable(form, Mappings.SelectDto)
                                                  .WithGlobalFilter(nameof(TestDto.StrVal), nameof(TestDto.NullStr))
                                                  .BuildAsync(TestContext.Current.CancellationToken);
        // Assert
        Assert.Equal(recordsTotal, result.RecordsTotal);
        Assert.Equal(entities.Count, result.RecordsFiltered);
        Assert.Equal(entities.Select(x => x.Id), result.Data.Select(x => x.Id));
    }
}

