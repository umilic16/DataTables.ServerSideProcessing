namespace Tests.Data;

public class TestDto
{
    public int Id { get; set; }
    public int IntVal { get; set; }
    public int? NullInt { get; set; }
    public decimal DecVal { get; set; }
    public decimal? NullDec { get; set; }
    public DateTime DtVal { get; set; }
    public DateTime? NullDt { get; set; }
    public DateOnly DoVal { get; set; }
    public DateOnly? NullDo { get; set; }
    public DateTimeOffset DtoVal { get; set; }
    public DateTimeOffset? NullDto { get; set; }
    public Something EnumVal { get; set; }
    public Something? NullableEnum { get; set; }
    public bool BoolVal { get; set; }
    public bool? NullableBool { get; set; }
    public string StrVal { get; set; } = string.Empty;
    public string? NullStr { get; set; }
}
