using System.Linq.Expressions;
using System.Reflection;

namespace JLib.Helper;

public static class ExpressionHelper
{
    public static Expression<Func<TSource, TKey?>> ToNullable<TSource, TKey>(
        this Expression<Func<TSource, TKey>> expression)
        where TKey : struct
    {
        var param = Expression.Parameter(typeof(TSource), expression.Parameters[0].Name);
        var body = Expression.Convert(expression.Body, typeof(TKey?));
        return Expression.Lambda<Func<TSource, TKey?>>(body, param);
    }

    public static PropertyInfo GetPropertyInfo<TSource, TValue>(this Expression<Func<TSource, TValue>> propertyLambda)
    {
        Type type = typeof(TSource);

        MemberExpression member = propertyLambda.Body switch
        {
            MemberExpression directMember => directMember,
            UnaryExpression { Operand: MemberExpression indirectMember } => indirectMember,
            _ => throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.")
        };

        if (member.Member is not PropertyInfo propInfo)
            throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");

        if (propInfo.ReflectedType is null)
            throw new ArgumentException($"Expression '{propertyLambda}' has no ReflectedType.");


        var tValue = typeof(TValue);
        var propType = tValue.IsGenericType && tValue.GetGenericTypeDefinition() == typeof(Nullable<>)
            ? tValue.GenericTypeArguments.First()
            : tValue;

        if (type != propInfo.ReflectedType && (propInfo.ReflectedType.IsInterface &&
                                               !type.Implements(propInfo.ReflectedType))
                                           && !type.IsSubclassOf(propInfo.ReflectedType))
            throw new ArgumentException(
                $"Expression '{propertyLambda}' refers to a property that is not from type {type}.");

        return propInfo;
    }
}