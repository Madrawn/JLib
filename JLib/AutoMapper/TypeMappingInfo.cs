using JLib.Reflection;

namespace JLib.AutoMapper;

public record ExplicitTypeMappingInfo(ITypeValueType Source, ITypeValueType Destination, MappingDataProviderMode DataProviderMode);

public enum MappingDataProviderMode
{
    Disabled,
    Read,
}