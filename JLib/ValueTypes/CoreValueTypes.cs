using JLib.AutoMapper;

namespace JLib.ValueTypes;

public abstract record StringValueType(string Value) : ValueType<string>(Value);

public abstract record GuidValueType(Guid Value) : ValueType<Guid>(Value);


public record Prefix(string Value) : StringValueType(Value);

public record PropertyPrefix(string Value) : Prefix(Value), IPropertyResolver
{
    public static implicit operator PropertyPrefix?(string? value)
        => value is null ? null : new(value);

    public string GetComparisonString(string propertyName)
    {
        var i = propertyName.IndexOf(propertyName, StringComparison.Ordinal);
        return i == -1
            ? propertyName
            : propertyName[(i + 1)..];
    }
}
public record PropertySuffix(string Value) : Prefix(Value), IPropertyResolver
{
    public static implicit operator PropertySuffix?(string? value)
        => value is null ? null : new(value);

    public string GetComparisonString(string propertyName)
    {

        var i = propertyName.LastIndexOf(propertyName, StringComparison.Ordinal);
        return
            i == -1
                ? propertyName
                : propertyName[..(i + 1)];
    }
}
/// <summary>
/// the string that separates a prefix from the rest of the propertyName
/// </summary>
/// <param name="Value"></param>
public record PropertyPrefixSeparator(string Value) : StringValueType(Value), IPropertyResolver
{
    public static implicit operator PropertyPrefixSeparator?(string? value)
        => value is null ? null : new(value);

    public string GetComparisonString(string propertyName)
    {
        var i = propertyName.IndexOf(Value, StringComparison.Ordinal);
        var result = i == -1
            ? propertyName
            : propertyName[(i + 1)..];
        return result;
    }
}
public record ClassPrefix(string Value) : Prefix(Value);