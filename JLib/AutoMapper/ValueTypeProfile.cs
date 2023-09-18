using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using JLib.Attributes;
using JLib.Exceptions;
using Microsoft.Extensions.Primitives;

namespace JLib.AutoMapper;

public class CustomMapperProfile : Profile
{
}

/// <summary>
/// Provides Mappings for all value types.<br/>
/// to enable this, an ExpressionVisitor is used which replaces a temporary placeholder with the valid constructor.
/// this is required, since we can not call a ctor with parameters of a generic type argument
/// </summary>
public class ValueTypeProfile : Profile
{
    /// <summary>
    /// replaces the valueType Placeholder Method with the valid constructor
    /// </summary>
    /// <typeparam name="TValueType"></typeparam>
    /// <typeparam name="TNative"></typeparam>
    private class CtorReplacementExpressionVisitor<TValueType, TNative> : ExpressionVisitor
    {
        public static TValueType CtorPlaceholder(TNative Value) =>
            throw new InvalidSetupException("this should have been replaced");

        private static readonly MethodInfo PlaceholderMi = typeof(CtorReplacementExpressionVisitor<TValueType, TNative>).GetMethod(nameof(CtorPlaceholder))
                                                            ?? throw new InvalidSetupException("PlaceholderMethodInfo not found");

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method != PlaceholderMi)
                return node;

            var ctor = typeof(TValueType).GetConstructor(new[] { typeof(TNative) })
                ?? throw new InvalidSetupException("valueType ctor not found");
            return Expression.New(ctor, node.Arguments);
        }

        public Expression<Func<TNative, TValueType>> Visit(Expression<Func<TNative, TValueType>> expression)
            => (Expression<Func<TNative, TValueType>>)base.Visit(expression);
    }

    private static class ClassValueTypeConversions<TValueType, TNative>
        where TValueType : ValueType<TNative>
        where TNative : class
    {

        public static void AddMapping(Profile profile)
        {
            profile.CreateMap<TValueType?, TNative?>().ConvertUsing(vt => vt == null ? null : vt.Value);
            profile.CreateMap<TNative?, TValueType?>().ConvertUsing(
                new CtorReplacementExpressionVisitor<TValueType?, TNative?>().Visit(
                v => v == null ? null :
                    CtorReplacementExpressionVisitor<TValueType?, TNative?>.CtorPlaceholder(v)
                ));
        }
    }

    private static class StructValueTypeConversions<TValueType, TNative>
        where TValueType : ValueType<TNative>
        where TNative : struct
    {

        public static void AddMapping(Profile profile)
        {
            profile.CreateMap<TValueType, TNative>().ConvertUsing(vt => vt.Value);
            profile.CreateMap<TNative, TValueType>().ConvertUsing(
                new CtorReplacementExpressionVisitor<TValueType, TNative>().Visit(
                    v =>
                        CtorReplacementExpressionVisitor<TValueType, TNative>.CtorPlaceholder(v)
                ));

            profile.CreateMap<TValueType, TNative?>().ConvertUsing(vt => vt == null ? null : vt.Value);
            profile.CreateMap<TNative?, TValueType?>().ConvertUsing(
                new CtorReplacementExpressionVisitor<TValueType?, TNative?>().Visit(
                    v => v.HasValue
                        ? CtorReplacementExpressionVisitor<TValueType, TNative>.CtorPlaceholder(v.Value)
                        : null
                ));
        }
    }








    public ValueTypeProfile(ITypeCache cache)
    {
        foreach (var valueType in cache.All<Types.ValueType>(vt => vt is { Mapped: true }))
        {
            if (!valueType.NativeType.IsValueType)
            {
                var addMapping = typeof(ClassValueTypeConversions<,>).MakeGenericType(valueType.Value, valueType.NativeType)
                        .GetMethod(nameof(ClassValueTypeConversions<ValueType<Ignored>, Ignored>.AddMapping)) ??
                    throw new InvalidSetupException("AddProfileMethodNotFound");
                addMapping.Invoke(null, new object?[] { this });
            }
            else
            {
                var addMapping = typeof(StructValueTypeConversions<,>).MakeGenericType(valueType.Value, valueType.NativeType)
                                     .GetMethod(nameof(StructValueTypeConversions<ValueType<int>, int>.AddMapping)) ??
                                 throw new InvalidSetupException("AddProfileMethodNotFound");
                addMapping.Invoke(null, new object?[] { this });
            }
        }
    }
}