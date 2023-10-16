using JLib.Attributes;
using JLib.AutoMapper;
using JLib.Helper;
using System.Reflection;

namespace JLib;

public enum MappingDirection
{
    ThatToThis = 0b01,
    ThisToThat = 0b10,
    Bidirectional = ThatToThis | ThisToThat
}

public static class PropertyResolver
{
    public static SameNamePropertyResolver CaseSensitive = new();
    public static CaseInsensitivePropertyResolver IgnoreCase = new();

}

public class SameNamePropertyResolver : IPropertyResolver
{
    public string GetComparisonString(string propertyName)
        => propertyName;
}
public class CaseInsensitivePropertyResolver : IPropertyResolver
{
    public string GetComparisonString(string propertyName)
        => propertyName.ToLower();
}

/// <summary>
/// used by the <see cref="MappedDataObjectProfile"/> to check if 2 Properties should be mapped onto each other
/// </summary>
public interface IPropertyResolver
{
    string GetComparisonString(string propertyName);
}
public record ExplicitTypeMappingInfo(ITypeValueType Source, ITypeValueType Destination,
    MappingDataProviderMode DataProviderMode,
    IPropertyResolver[] SourcePropertyResolver,
    IPropertyResolver[] DestinationPropertyResolver);

public enum MappingDataProviderMode
{
    Disabled,
    Read,
    ReadWrite,
}