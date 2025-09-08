namespace Tests.Data;

public class TestEntity
{
    public int Id { get; set; }
    public int IntVal { get; set; }
    public int? NullableInt { get; set; }
    public decimal DecimalVal { get; set; }
    public decimal? NullableDecimal { get; set; }
    public DateTime DateTimeVal { get; set; }
    public DateTime? NullableDateTime { get; set; }
    public DateOnly DateOnlyVal { get; set; }
    public DateOnly? NullableDateOnly { get; set; }
    public Something EnumVal { get; set; }
    public Something? NullableEnum { get; set; }
    public bool BoolVal { get; set; }
    public bool? NullableBool { get; set; }
    public string StringVal { get; set; } = string.Empty;
    public string? NullableString { get; set; }
}

