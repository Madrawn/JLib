using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
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
public static class ValueType
{
    private class ConstantErrorValidationProfile<T> : IValidationProfile<T>
    {
        private readonly IExceptionProvider _ex;
        public ConstantErrorValidationProfile(IExceptionProvider ex)
        {
            _ex = ex;
        }
        public IExceptionProvider Validate(T? value) => _ex;
    }

    /// <summary>
    /// checks, whether the given <paramref name="value"/> is a valid <typeparamref name="TVt"/>. throws if it is not.
    /// </summary>
    /// <typeparam name="TVt">The specific <see cref="ValueType{T}"/> to validate the <paramref name="value"/> for</typeparam>
    /// <typeparam name="T">The native value of the <see cref="ValueType{T}"/></typeparam>
    /// <param name="value">the value to validate</param>
    /// <returns>an <see cref="IExceptionProvider"/> containing all validation errors. Use <see cref="IExceptionProvider.HasErrors"/> to check if the value is valid</returns>
    public static void Validate<TVt, T>(T value)
        where TVt : ValueType<T>
        => ValidationProfile<T>.Get(typeof(TVt)).Validate(value).ThrowIfNotEmpty();
    /// <summary>
    /// checks, whether the given <paramref name="value"/> is a valid <typeparamref name="TVt"/> and returns all validation errors
    /// </summary>
    /// <typeparam name="TVt">The specific <see cref="ValueType{T}"/> to validate the <paramref name="value"/> for</typeparam>
    /// <typeparam name="T">The native value of the <see cref="ValueType{T}"/></typeparam>
    /// <param name="value">the value to validate</param>
    /// <returns>an <see cref="IExceptionProvider"/> containing all validation errors. Use <see cref="IExceptionProvider.HasErrors"/> to check if the value is valid</returns>
    public static IExceptionProvider GetErrors<TVt, T>(T value)
        where TVt : ValueType<T>
        => ValidationProfile<T>.Get(typeof(TVt)).Validate(value);


    private enum DataMemberKind { Field, Property }

    /// <summary>
    /// wraps properties and fields in a common type to simplify validation
    /// </summary>
    private class DataMemberType
    {
        public string Name { get; }
        public MemberInfo MemberInfo { get; }
        public ValidationAttribute? ValidatorAttribute { get; }
        public PropertyInfo? PropertyInfo { get; }
        public FieldInfo? FieldInfo { get; }
        public Type DataType { get; }

        private DataMemberType(MemberInfo memberInfo)
        {
            MemberInfo = memberInfo;
            ValidatorAttribute = memberInfo.GetCustomAttribute<ValidationAttribute>();
            Name = memberInfo.Name;
            DataType = null!;
        }
        public DataMemberType(FieldInfo fieldInfo) : this((MemberInfo)fieldInfo)
        {
            FieldInfo = fieldInfo;
            DataType = fieldInfo.FieldType;
        }
        public DataMemberType(PropertyInfo propertyInfo) : this((MemberInfo)propertyInfo)
        {
            PropertyInfo = propertyInfo;
            DataType = propertyInfo.PropertyType;
        }
    }

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

    public T Value { get; init; }

}