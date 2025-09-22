using System.Linq.Expressions;

namespace Tests.Data;

public static class Mappings
{
    public static Expression<Func<TestEntity, TestDto>> SelectDto => entity =>
        new TestDto
        {
            Id = entity.Id,
            IntVal = entity.IntVal,
            NullInt = entity.NullableInt,
            DecVal = entity.DecimalVal,
            NullDec = entity.NullableDecimal,
            DtVal = entity.DateTimeVal,
            NullDt = entity.NullableDateTime,
            DoVal = entity.DateOnlyVal,
            NullDo = entity.NullableDateOnly,
            DtoVal = entity.DateTimeOffsetVal,
            NullDto = entity.NullableDateTimeOffset,
            EnumVal = entity.EnumVal,
            NullableEnum = entity.NullableEnum,
            BoolVal = entity.BoolVal,
            NullableBool = entity.NullableBool,
            StrVal = entity.StringVal,
            NullStr = entity.NullableString
        };
}
