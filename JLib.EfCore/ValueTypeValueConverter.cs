using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JLib.EfCore;
public class ClassValueTypeValueConverter<TValueType, TValue> : ValueConverter<TValueType?, TValue?>
    where TValueType : ValueType<TValue>
    where TValue : class
{
    private static Expression<Func<TValue?, TValueType?>> GetVtFactory()
    {
        var vtType = typeof(TValueType);
        var par = Expression.Parameter(typeof(TValue), "value");
        var ctor = vtType.GetConstructor(new[] { typeof(TValue) }) ?? throw new Exception("ctor not found");
        var ctorEx = Expression.New(ctor, par);
        var vNullValue = Expression.Constant(null, typeof(TValue));
        var tvtNullValue = Expression.Constant(null, typeof(TValueType));
        var compare = Expression.Equal(par, vNullValue);
        var returnEx = Expression.Label(typeof(TValueType));
        var ifEx = Expression.IfThen(
            compare,
            Expression.Return(returnEx, tvtNullValue));

        var body = Expression.Block(
            ifEx,
            Expression.Label(returnEx, ctorEx)
        );

        return Expression.Lambda<Func<TValue?, TValueType?>>(body, par);
    }
    public ClassValueTypeValueConverter() : base(x => x == null ? null : x.Value, GetVtFactory())
    {

    }
}
public class StructValueTypeValueConverter<TValueType, TValue> : ValueConverter<TValueType?, TValue?>
    where TValueType : ValueType<TValue>
    where TValue : struct
{
    private static Expression<Func<TValue?, TValueType?>> GetVtFactory()
    {
        var vtType = typeof(TValueType);
        var par = Expression.Parameter(typeof(TValue), "value");
        var ctor = vtType.GetConstructor(new[] { typeof(TValue) }) ?? throw new Exception("ctor not found");
        var ctorEx = Expression.New(ctor, par);
        var vNullValue = Expression.Constant(null, typeof(TValue));
        var tvtNullValue = Expression.Constant(null, typeof(TValueType));
        var compare = Expression.Equal(par, vNullValue);
        var returnEx = Expression.Label(typeof(TValueType));
        var ifEx = Expression.IfThen(
            compare,
            Expression.Return(returnEx, tvtNullValue));

        var body = Expression.Block(
            ifEx,
            Expression.Label(returnEx, ctorEx)
        );

        return Expression.Lambda<Func<TValue?, TValueType?>>(body, par);
    }
    public StructValueTypeValueConverter() : base(x => x == null ? null : x.Value, GetVtFactory())
    {

    }
}

public class StructNonNullableValueTypeValueConverter<TValueType, TValue> : ValueConverter<TValueType, TValue>
    where TValueType : ValueType<TValue>
    where TValue : struct
{
    private static Expression<Func<TValue, TValueType>> GetVtFactory()
    {
        var vtType = typeof(TValueType);
        var par = Expression.Parameter(typeof(TValue), "value");
        var ctor = vtType.GetConstructor(new[] { typeof(TValue) }) ?? throw new Exception("ctor not found");
        var ctorEx = Expression.New(ctor, par);
        return Expression.Lambda<Func<TValue, TValueType>>(ctorEx, par);
    }
    public StructNonNullableValueTypeValueConverter() : base(x => x.Value, GetVtFactory())
    {

    }
}