using System.Reflection;
using JLib.Attributes;
using JLib.Exceptions;
using Serilog.Events;

namespace JLib;


public interface IInitializedType : ITypeValueType
{
    void Initialize(IExceptionManager exceptions);
}

public interface IValidatedType : ITypeValueType
{
    void Validate(ITypeCache cache, TvtValidator validator);
}

public interface IMappedDataObjectType : ITypeValueType
{
    EntityType SourceEntity { get; }
    PropertyPrefix? PropertyPrefix { get; }
    bool ReverseMap { get; }
}
public interface ITypeValueType
{
    public string Name => Value.Name;
    Type Value { get; }
    public bool HasCustomAutoMapperProfile { get; }
}
[Unmapped]
public abstract partial record TypeValueType(Type Value) : ValueType<Type>(Value), ITypeValueType
{
    public string Name => Value.Name;

    protected InvalidTypeException NewInvalidTypeException(string message)
        => new(GetType(), Value, message);

    public bool HasCustomAutoMapperProfile => Value.GetCustomAttributes().Any(a => a is ICustomProfileAttribute);

}