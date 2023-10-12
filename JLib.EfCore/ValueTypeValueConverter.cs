using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JLib.EfCore;
public class ClassValueTypeValueConverter<TValueType, TValue> : ValueConverter<TValueType, TValue?>
    where TValueType : ValueType<TValue>
    where TValue : class
{
    private static Expression<Func<TValue?, TValueType>> GetVtFactory()
    {
        var vtType = typeof(TValueType);
        var par = Expression.Parameter(typeof(TValue), "value");
        var ctor = vtType.GetConstructor(new[] { typeof(TValue) }) ?? throw new Exception("ctor not found");
        var ctorEx = Expression.New(ctor, par);
        var nullValue = Expression.Constant(null, typeof(TValue));
        var compare = Expression.Equal(par, nullValue);
        var ifEx = Expression.IfThenElse(
            compare,
            Expression.Constant(null, typeof(TValue)),
            ctorEx);
        return Expression.Lambda<Func<TValue?, TValueType>>(ifEx, par);
    }
    public ClassValueTypeValueConverter() : base(x => x.Value, GetVtFactory())
    {

    }
}
public class StructValueTypeValueConverter<TValueType, TValue> : ValueConverter<TValueType, TValue?>
    where TValueType : ValueType<TValue>
    where TValue : struct
{
    private static Expression<Func<TValue?, TValueType>> GetVtFactory()
    {
        var vtType = typeof(TValueType);
        var par = Expression.Parameter(typeof(TValue), "value");
        var ctor = vtType.GetConstructor(new[] { typeof(TValue) }) ?? throw new Exception("ctor not found");
        var ctorEx = Expression.New(ctor, par);
        var nullValue = Expression.Constant(null, typeof(TValue?));
        var compare = Expression.Equal(par, nullValue);
        var ifEx = Expression.IfThenElse(
            compare,
            Expression.Constant(null, typeof(TValue)),
            ctorEx);
        return Expression.Lambda<Func<TValue?, TValueType>>(ifEx, par);
    }
    public StructValueTypeValueConverter() : base(x => x.Value, GetVtFactory())
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