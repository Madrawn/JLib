using JLib.DataProvider;

namespace JLib.HotChocolate.Reflection;

public class DataProviderInfoGdo
{
    public ServiceInfoGdo DataProvider { get; }
    public ServiceInfoGdo SourceDataProvider { get; }

    public DataProviderInfoGdo(DataObjectType dataObjectType, IGraphQlReflectionEndpointCache gdoCache)
    {
        DataProvider = gdoCache.ToServiceInfoGdo(typeof(IDataProviderR<>).MakeGenericType(dataObjectType.Value));
        SourceDataProvider = gdoCache.ToServiceInfoGdo(typeof(ISourceDataProviderR<>).MakeGenericType(dataObjectType.Value));
    }
}