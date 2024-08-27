using System.Reflection;
using JLib.Exceptions;
using JLib.ValueTypes;

namespace JLib.Reflection;

/// <summary>
/// allows a <see cref="ITypeValueType"/> to run code after the navigation has been initialized but before it will be validated
/// <br/>can be used to set properties which are derived from Attributes on navigated types
/// </summary>
public interface IPostNavigationInitializedType : ITypeValueType
{
    /// <summary>
    /// called, after <see cref="NavigatingTypeValueType.Navigate{T}"/> has been initialized
    /// </summary>
    void Initialize(ITypeCache cache, ExceptionBuilder exceptions);
}

/// <summary>
/// marks the <see cref="TypeValueType"/>
/// </summary>
public interface IValidatedType : ITypeValueType
{
    void Validate(ITypeCache cache, TypeValidationContext value);
}

public interface ITypeValueType
{
    public string Name => Value.Name;
    Type Value { get; }
    public bool HasCustomAutoMapperProfile { get; }
}

/// <summary>
/// integrates with
/// <list type="bullet">
/// <item><seealso cref="NavigatingTypeValueType"/></item>
/// <item><seealso cref="IPostNavigationInitializedType"/></item>
/// <item><seealso cref="IValidatedType"/></item>
/// <item><seealso cref="ITypeValueType"/></item>
/// </list>
/// </summary>
[Unmapped]
public abstract record TypeValueType(Type Value) : ValueType<Type>(Value), ITypeValueType
{
    public string Name => Value.Name;

    protected InvalidTypeException NewInvalidTypeException(string message)
        => new(GetType(), Value, message);

    public bool HasCustomAutoMapperProfile => Value.GetCustomAttributes().Any(a => a is IDisableAutoProfileAttribute);
}