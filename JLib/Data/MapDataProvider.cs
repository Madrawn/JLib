using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace JLib.Data;

public class MapDataProvider<TFrom, TTo> : IDataProviderR<TTo>
{
    private readonly IDataProviderR<TFrom> _provider;
    private readonly IConfigurationProvider _config;

    public MapDataProvider(IDataProviderR<TFrom> provider, IConfigurationProvider config)
    {
        _provider = provider;
        _config = config;
    }
    public IQueryable<TTo> Get()
        => _provider.Get().ProjectTo<TTo>(_config);
}