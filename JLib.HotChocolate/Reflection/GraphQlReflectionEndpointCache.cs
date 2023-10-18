using System.Diagnostics.CodeAnalysis;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.HotChocolate.Reflection;

public interface IGraphQlReflectionEndpointCache
{
    [return: NotNullIfNotNull("type")]
    TypeGdo? ToGdo(ITypeValueType? type)
        => ToGdo(type?.Value);

    [return: NotNullIfNotNull("type")]
    TypeGdo? ToGdo(Type? type);

    [return: NotNullIfNotNull("type")]
    ServiceInfoGdo? ToServiceInfoGdo(Type? type);

    [return: NotNullIfNotNull("dataObject")]
    DataProviderInfoGdo? ToDataProviderInfoGdo(DataObjectType? dataObject);
}
public class GraphQlReflectionEndpointCache : IGraphQlReflectionEndpointCache, IDisposable
{
    private readonly IServiceProvider _provider;
    private readonly ITypeCache _typeCache;
    private readonly Dictionary<Type, TypeGdo> _graphQlTypes = new();
    private readonly Dictionary<Type, ServiceInfoGdo> _serviceInfos = new();
    private readonly Dictionary<DataObjectType, DataProviderInfoGdo> _dataProviders = new();
    private readonly IServiceScope _scope;

    public GraphQlReflectionEndpointCache(IServiceProvider provider, ITypeCache typeCache)
    {
        _provider = provider;
        _typeCache = typeCache;
        _scope = provider.CreateScope();
        _provider = _scope.ServiceProvider;
    }

    [return: NotNullIfNotNull("type")]
    public TypeGdo? ToGdo(Type? type)
        => type is null ? null : _graphQlTypes.GetValueOrAdd(type, t => new(t));
    [return: NotNullIfNotNull("type")]
    public ServiceInfoGdo? ToServiceInfoGdo(Type? type)
    {
        return type is null ? null : _serviceInfos.GetValueOrAdd(type, t => new(t, _provider, _typeCache, this));
    }
    [return: NotNullIfNotNull("dataObject")]
    public DataProviderInfoGdo? ToDataProviderInfoGdo(DataObjectType? dataObject)
    {
        return dataObject is null ? null : _dataProviders.GetValueOrAdd(dataObject, t => new(t, this));
    }

    public void Dispose() => _scope.Dispose();
}