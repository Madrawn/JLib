using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.ValueTypes;

/// <summary>
/// This property is used to validate this value type
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ValidationAttribute : Attribute { }


/// <summary>
/// Contains Utility methods to validate value types
/// </summary>
public static partial class ValueType
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TVt"></typeparam>
    /// <typeparam name="T"></typeparam>
    public static Exception? ValidateValueTypeSetup<TVt, T>()
        where TVt : ValueType<T>
    {
        try
        {
            ValidationProfile<T>.Get(typeof(TVt));
        }
        catch (Exception e)
        {
            return new($"initialization of ValueType {typeof(TVt).FullName(true)} Failed.", e);
        }

        return null;
    }
    /// <summary>
    /// Checks, whether there have been any errors made while implementing the given value type.
    /// </summary>
    /// <param name="valueType"></param>
    /// <returns></returns>
    /// <remarks>executes <see cref="ValidateValueTypeSetup{TVt,T}"/> with the types extracted form <paramref name="valueType"/></remarks>
    public static Exception? ValidateValueTypeSetup(Type valueType)
    {

        var baseType = valueType.GetAnyBaseType<ValueType<Ignored>>();
        if (baseType is null)
            return new($"The given type {valueType.FullName(true)} is not derived from {typeof(ValueType<>).FullName(true)}");

        var nativeType = baseType.GenericTypeArguments.First();

        var mi = typeof(ValueType).GetMethod(nameof(ValidateValueTypeSetup), 2, Array.Empty<Type>())
            ?.MakeGenericMethod(valueType, nativeType);
        if (mi is null)
            return new ArgumentException($"Internal Error: `ValidateValueTypeSetup` could not be found for valueType {valueType.FullName(true)}");

        try
        {
            return mi.Invoke(null, Array.Empty<object>()) as Exception;
        }
        catch (Exception e)
        {
            return new ArgumentException($"Internal Error: `ValidateValueTypeSetup<{valueType.FullName()}, {nativeType.FullName()}>()` invocation failed", e);
        }
    }

    /// <summary>
    /// checks, whether the given <paramref name="value"/> is a valid <typeparamref name="TVt"/>.
    /// </summary>
    /// <typeparam name="TVt">The specific <see cref="ValueType{T}"/> to validate the <paramref name="value"/> for</typeparam>
    /// <typeparam name="T">The native value of the <see cref="ValueType{T}"/></typeparam>
    /// <param name="value">the value to validate</param>
    /// <returns>true if the given <paramref name="value"/> is a valid <typeparamref name="TVt"/>, otherwise false</returns>
    public static bool Validate<TVt, T>(T value)
        where TVt : ValueType<T>
        => ValidationProfile<T>.Get(typeof(TVt)).Validate(value).HasErrors() == false;
    /// <summary>
    /// checks, whether the given <paramref name="value"/> is a valid <typeparamref name="TVt"/> and returns all validation errors
    /// </summary>
    /// <typeparam name="TVt">The specific <see cref="ValueType{T}"/> to validate the <paramref name="value"/> for</typeparam>
    /// <typeparam name="T">The native value of the <see cref="ValueType{T}"/></typeparam>
    /// <param name="value">the value to validate</param>
    /// <returns>an <see cref="IExceptionProvider"/> containing all validation errors. Use <see cref="IExceptionProvider.HasErrors"/> to check if the value is valid</returns>
    public static IExceptionProvider GetErrors<TVt, T>(T? value)
        where TVt : ValueType<T>
        => ValidationProfile<T>.Get(typeof(TVt)).Validate(value);

    #region try create
    /// <summary>
    /// Checks whether <paramref name="value"/> is a valid <typeparamref name="TVt"/> and returns the value if it is valid, otherwise null and the validation errors via <paramref name="validationErrors"/>.
    /// </summary>
    /// <typeparam name="TVt">The specific <see cref="ValueType{T}"/> to create the <paramref name="value"/> for</typeparam>
    /// <typeparam name="T">The native value of the <typeparamref name="TVt"/></typeparam>
    /// <param name="value">the value to create to a new <typeparamref name="TVt"/></param>
    /// <param name="validationErrors">the errors, if any when tryCreate failed.</param>
    /// <returns>a new instance of <typeparamref name="TVt"/> containing <paramref name="value"/> as it's value ot null, if the validation failed.</returns>
    public static TVt? TryCreate<TVt, T>(T? value, out IExceptionProvider validationErrors)
        where TVt : ValueType<T>
    {
        validationErrors = EmptyExceptionProvider.Instance;
        if (value is null)
            return null;
        validationErrors = GetErrors<TVt, T>(value);
        if (validationErrors.HasErrors())
            return null;

        return validationErrors.HasErrors()
            ? null
            : Create<TVt, T>(value);
    }
    /// <summary>
    /// Checks whether <paramref name="value"/> is a valid <typeparamref name="TVt"/> and returns the value if it is valid, otherwise null and the validation errors via <paramref name="validationErrors"/>.
    /// </summary>
    /// <typeparam name="TVt">The specific <see cref="ValueType{T}"/> to create the <paramref name="value"/> for</typeparam>
    /// <typeparam name="T">The native value of the <typeparamref name="TVt"/></typeparam>
    /// <param name="value">the value to create to a new <typeparamref name="TVt"/></param>
    /// <param name="validationErrors">the errors, if any when tryCreate failed.</param>
    /// <returns>a new instance of <typeparamref name="TVt"/> containing <paramref name="value"/> as it's value ot null, if the validation failed.</returns>
    public static TVt? TryCreate<TVt, T>(T? value, out IExceptionProvider validationErrors)
        where TVt : ValueType<T>
        where T : struct
    {
        validationErrors = EmptyExceptionProvider.Instance;
        if (value.HasValue is false)
            return null;
        validationErrors = GetErrors<TVt, T>(value.Value);

        return validationErrors.HasErrors()
            ? null
            : CreateNullable<TVt, T>(value);
    }
    #endregion

    #region create

    private static readonly ConcurrentDictionary<string, Delegate> CompiledExpressionCache = new();

    /// <summary>
    /// Checks whether <paramref name="value"/> is a valid <typeparamref name="TVt"/> and returns the value if it is valid, otherwise throws an <see cref="AggregateException"/>.
    /// </summary>
    /// <typeparam name="TVt">The specific <see cref="ValueType{T}"/> to create the <paramref name="value"/> for</typeparam>
    /// <typeparam name="T">The native value of the <typeparamref name="TVt"/></typeparam>
    /// <param name="value">the value to create to a new <typeparamref name="TVt"/></param>
    /// <exception cref="AggregateException"></exception>
    /// <returns>a new instance of <typeparamref name="TVt"/> containing <paramref name="value"/> as it's value ot null, if the validation failed.</returns>
    [return: NotNullIfNotNull("value")]
    public static TVt? CreateNullable<TVt, T>(T? value)
        where TVt : ValueType<T>
        => value is null
            ? null
            : Create<TVt, T>(value);

    /// <summary>
    /// Checks whether <paramref name="value"/> is a valid <typeparamref name="TVt"/> and returns the value if it is valid, otherwise throws an <see cref="AggregateException"/>.<br/>
    /// values may be null. if they are, null will be returned.
    /// </summary>
    /// <typeparam name="TVt">The specific <see cref="ValueType{T}"/> to create the <paramref name="value"/> for</typeparam>
    /// <typeparam name="T">The native value of the <typeparamref name="TVt"/></typeparam>
    /// <param name="value">the value to create to a new <typeparamref name="TVt"/></param>
    /// <exception cref="AggregateException"></exception>
    /// <returns>a new instance of <typeparamref name="TVt"/> containing <paramref name="value"/> as it's value ot null, if the validation failed.</returns>
    [return: NotNullIfNotNull("value")]
    public static TVt? Create<TVt, T>(T? value)
        where TVt : ValueType<T>
    => typeof(T).IsValueType
        ? CompiledExpressionCache.GetOrAdd(
            GetExpressionCacheKey<TVt>(false),
            _ => FactoryExpressions.ForAnyType<TVt, T>(false).Compile()
        ).DynamicInvoke(value) as TVt // let's hope this is not too slow
        : CompiledExpressionCache.GetOrAdd(
            GetExpressionCacheKey<TVt>(false),
            _ => FactoryExpressions.ForAnyType<TVt, T>(false).Compile()
        ).CastTo<Func<T?, TVt?>>().Invoke(value);

    /// <summary>
    /// Checks whether <paramref name="value"/> is a valid <typeparamref name="TVt"/> and returns the value if it is valid, otherwise throws an <see cref="AggregateException"/>.<br/>
    /// values may be null. if they are, null will be returned.
    /// </summary>
    /// <typeparam name="TVt">The specific <see cref="ValueType{T}"/> to create the <paramref name="value"/> for</typeparam>
    /// <typeparam name="T">The native value of the <typeparamref name="TVt"/></typeparam>
    /// <param name="value">the value to create to a new <typeparamref name="TVt"/></param>
    /// <exception cref="AggregateException"></exception>
    /// <returns>a new instance of <typeparamref name="TVt"/> containing <paramref name="value"/> as it's value ot null, if the validation failed.</returns>
    [return: NotNullIfNotNull("value")]
    public static TVt? CreateNullable<TVt, T>(T? value)
        where TVt : ValueType<T>
        where T : struct
        // todo: use compiled expression to improve runtime performance
        => value is null
            ? null
            : CompiledExpressionCache.GetOrAdd(
                GetExpressionCacheKey<TVt>(true),
                _ => FactoryExpressions.ForNullableStruct<TVt, T>().Compile()
            ).CastTo<Func<T?, TVt?>>().Invoke(value);
    #endregion

    private static string GetExpressionCacheKey<TVt>(bool nullable)
        => GetExpressionCacheKey(typeof(TVt), nullable);
    private static string GetExpressionCacheKey(Type valueType, bool nullable)
        => $"{valueType.FullName(true)}{(nullable ? "Nullable" : "")}";

}

/// <summary>
/// Base class for all value types<br/>
/// JLib.ValueTypes.Mapping adds Automapper and native json support for System.Text.Json
/// </summary>
public abstract record ValueType<T>
{
    /// <summary>
    /// Base class for all value types<br/>
    /// JLib.ValueTypes.Mapping adds Automapper and native json support for System.Text.Json
    /// </summary>
    protected ValueType(T Value)
    {
        this.Value = Value;
        var profile = ValidationProfile<T>.Get(GetType());
        var errors = profile.Validate(Value);
        errors.ThrowIfNotEmpty();
    }

    public virtual void Deconstruct(out T value)
    {
        value = Value;
    }

    /// <summary>
    /// The value of the ValueType
    /// </summary>
    public T Value { get; init; }

}