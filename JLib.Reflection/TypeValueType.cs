using System.Reflection;
using JLib.Exceptions;
using JLib.Reflection.Attributes;
using JLib.ValueTypes;

namespace JLib.Reflection;

/// <summary>
/// allows a <see cref="ITypeValueType"/> to run code after the navigation has been initialized but before it will be validated
/// <br/>can be used to set properties which are derived from Attributes on navigated types
/// </summary>
public interface IPostNavigationInitializedType : ITypeValueType
{
    void Initialize(ITypeCache cache, IExceptionManager exceptions);
}

public interface IValidatedType : ITypeValueType
{
    void Validate(ITypeCache cache, TvtValidator value);
}

public interface ITypeValueType
{
    public string Name => Value.Name;
    Type Value { get; }
    public bool HasCustomAutoMapperProfile { get; }
}

[Unmapped]
public abstract record TypeValueType(Type Value) : ValueType<Type>(Value), ITypeValueType
{
    public string Name => Value.Name;

    protected InvalidTypeException NewInvalidTypeException(string message)
        => new(GetType(), Value, message);

    public bool HasCustomAutoMapperProfile => Value.GetCustomAttributes().Any(a => a is IDisableAutoProfileAttribute);
}