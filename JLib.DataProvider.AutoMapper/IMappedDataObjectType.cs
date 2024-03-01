namespace JLib.DataProvider.AutoMapper;

/// <summary>
/// the <see cref="MappedDataObjectProfile"/> will create a map for each <see cref="MappingInfo"/>
/// </summary>
public interface IMappedDataObjectType : IDataObjectType
{
    ExplicitTypeMappingInfo[] MappingInfo { get; }
}