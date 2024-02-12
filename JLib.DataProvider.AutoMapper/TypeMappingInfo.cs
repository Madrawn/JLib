using JLib.Reflection;

namespace JLib.DataProvider.AutoMapper;

public record ExplicitTypeMappingInfo(
    ITypeValueType Source,
    ITypeValueType Destination,
    MappingDataProviderMode DataProviderMode);

public enum MappingDataProviderMode
{
    Disabled,
    Read,
}