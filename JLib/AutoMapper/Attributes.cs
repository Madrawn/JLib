using JLib.ValueTypes;

namespace JLib.AutoMapper;

/// <summary>
/// used for entity mapping in conjunction with any <see cref="IMappedDataObjectType"/>.
/// <br/>Enables the Profile to ignore the separator when resolving the correlated properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PropertyPrefixAttribute : Attribute
{
    public PropertyPrefix Prefix { get; }

    public PropertyPrefixAttribute(string prefix)
    {
        Prefix = new(prefix);
    }
}

/// <summary>
/// used for entity mapping in conjunction with any <see cref="IMappedDataObjectType"/>.
/// <br/>Enables the Profile to ignore the separator when resolving the correlated properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PropertyPrefixSeparatorAttribute : Attribute
{
    public PropertyPrefixSeparator Separator { get; }

    public PropertyPrefixSeparatorAttribute(string separator)
    {
        Separator = new(separator);
    }
}