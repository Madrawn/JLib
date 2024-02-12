using JLib.DataProvider;
using JLib.Helper;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.HotChocolate.Reflection;

public class ServiceInfoGdo
{
    public TypeGdo Service { get; }
    public TypeGdo? Implementation { get; }

    private readonly Lazy<List<DataProviderInfoGdo>?> _referencedDataProviders;
    public List<DataProviderInfoGdo>? ReferencedDataProviders => _referencedDataProviders.Value;

    public ServiceInfoGdo(Type service, IServiceProvider provider, ITypeCache typeCache, IGraphQlReflectionEndpointCache gdoCache)
    {
        var instance = provider.GetService(service);
        if (instance == null)
        {
            using var scope = provider.CreateScope();
            instance = scope.ServiceProvider.GetService(service);
        }
        Service = gdoCache.ToGdo(service);
        Implementation = gdoCache.ToGdo(instance?.GetType());
        _referencedDataProviders = new(() => instance
            ?.GetType()
            .GetConstructors()
            .First()
            .GetParameters()
            .Select(p => p.ParameterType.GetAnyInterface<IDataProviderR<IDataObject>>())
            .WhereNotNull()
            .Select(t => t.GenericTypeArguments.First())
            .Select(typeCache.Get<DataObjectType>)
            .Select(gdoCache.ToDataProviderInfoGdo)
            .WhereNotNull()
            .ToList());
    }

}