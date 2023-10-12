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

public class SameNamePropertyResolver : IPropertyResolver
{
    public static SameNamePropertyResolver Instance = new();
    public bool MapProperty(PropertyInfo sourceProperty, PropertyInfo destinationProperty)
        => sourceProperty.Name == destinationProperty.Name;
}

/// <summary>
/// used by the <see cref="MappedDataObjectProfile"/> to check if 2 Properties should be mapped onto each other
/// </summary>
public interface IPropertyResolver
{
    bool MapProperty(PropertyInfo sourceProperty, PropertyInfo destinationProperty);
}

/// <summary>
/// generated from <see cref="TypeMappingInfo"/>
/// </summary>
/// <param name="Source"></param>
/// <param name="Destination"></param>
/// <param name="PropertyMatcher"></param>
public record ExplicitTypeMappingInfo(ITypeValueType Source, ITypeValueType Destination,
    MappingDataProviderMode DataProviderMode,
    IPropertyResolver[] PropertyMatcher);

public enum MappingDataProviderMode
{
    Disabled,
    Read,
    ReadWrite,
}
/// <summary>
/// used by the <see cref="MappedDataObjectProfile"/> to  create maps
/// </summary>
public record TypeMappingInfo(DataObjectType OtherObject, MappingDirection Direction,
    MappingDataProviderMode ThisDataProviderMode, MappingDataProviderMode OtherDataProviderMode, IPropertyResolver[] PropertyMatcher)
{
    public IEnumerable<ExplicitTypeMappingInfo> GetMappingInfosFor(ITypeValueType tvt)
    {
        var res = new List<ExplicitTypeMappingInfo>();
        if (tvt.HasCustomAutoMapperProfile
            || OtherObject.HasCustomAutoMapperProfile
            || tvt.Value.HasCustomAttribute<UnmappedAttribute>()
            || OtherObject.Value.HasCustomAttribute<UnmappedAttribute>())
            return res;

        if (Direction.HasFlag(MappingDirection.ThatToThis))
            res.Add(new(OtherObject, tvt, ThisDataProviderMode, PropertyMatcher));
        if (Direction.HasFlag(MappingDirection.ThisToThat))
            res.Add(new(tvt, OtherObject, OtherDataProviderMode, PropertyMatcher));
        return res;

    }
}