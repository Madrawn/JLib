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
    /// <summary>
    /// validates the given <paramref name="value"/> against the <paramref name="cache"/>
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="value"></param>
    void Validate(ITypeCache cache, TypeValidationContext value);
}
/// <summary>
/// Interface for <see cref="TypeValueType"/>s which expands the possibilities when designing type argument constraints.
/// </summary>
public interface ITypeValueType
{
    /// <summary>
    /// <see cref="Type.Name"/>
    /// </summary>
    public string Name => Value.Name;
    /// <summary>
    /// <inheritdoc cref="ValueType{T}.Value"/>
    /// </summary>
    Type Value { get; }
    /// <summary>
    /// Indicates whether the automated Automapper profile generation for this type is enabled or not.
    /// </summary>
    public bool HasCustomAutoMapperProfile { get; }
}

/// <summary>
/// a <see cref="ValueType{T}"/> for <see cref="Type"/>s
/// integrates with the <see cref="ITypeCache"/>
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
    /// <summary>
    /// <inheritdoc cref="ITypeValueType.Name"/>
    /// </summary>
    public string Name => Value.Name;

    /// <summary>
    /// creates a new, populated <see cref="InvalidTypeException"/>
    /// </summary>
    /// <param name="message">the exception message</param>
    protected InvalidTypeException NewInvalidTypeException(string message)
        => new(GetType(), Value, message);

    /// <summary>
    /// <inheritdoc cref="ITypeValueType.HasCustomAutoMapperProfile"/>
    /// </summary>
    public bool HasCustomAutoMapperProfile => Value.GetCustomAttributes().Any(a => a is IDisableAutoProfileAttribute);
}