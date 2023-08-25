using JLib.Exceptions;

namespace JLib;

public interface IPostInitValidatedType
{
    void PostInitValidation(ITypeCache cache);
}

public abstract record TypeValueType(Type Value) : ValueType<Type>(Value)
{
    public string Name => Value.Name;

    protected InvalidTypeException CreateInvalidTypeException(string message)
        => new(GetType(), Value, message);
}

public static class Types
{
}


public record NavigationPropertyName(string Value) : StringValueType(Value)
{
    public static implicit operator NavigationPropertyName(string value)
        => new(value);

}
