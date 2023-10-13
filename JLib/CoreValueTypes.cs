using System.Reflection;

namespace JLib;

public abstract record StringValueType(string Value) : ValueType<string>(Value);

public abstract record GuidValueType(Guid Value) : ValueType<Guid>(Value);


public record Prefix(string Value) : StringValueType(Value);

public record PropertyPrefix(string Value) : Prefix(Value), IPropertyResolver
{
    public static implicit operator PropertyPrefix?(string? value)
        => value is null ? null : new(value);

    public bool MapProperty(PropertyInfo sourceProperty, PropertyInfo destinationProperty)
        => sourceProperty.Name.Replace(Value, "") == destinationProperty.Name.Replace(Value, "");
}
/// <summary>
/// the string that separates a prefix from the rest of the propertyName
/// </summary>
/// <param name="Value"></param>
public record PropertyPrefixSeparator(string Value) : StringValueType(Value), IPropertyResolver
{
    public static implicit operator PropertyPrefixSeparator?(string? value)
        => value is null ? null : new(value);

    public bool MapProperty(PropertyInfo sourceProperty, PropertyInfo destinationProperty)
    {
        return RemovePrefix(sourceProperty.Name) == RemovePrefix(destinationProperty.Name);
        string RemovePrefix(string propName)
        {
            var i = propName.IndexOf(Value, StringComparison.Ordinal);
            return i == -1
                ? propName
                : propName[..i];
        }
    }
}
public record ClassPrefix(string Value) : Prefix(Value);