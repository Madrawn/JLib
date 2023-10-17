using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace JLib.Data;

public class MapDataProviderR<TFrom, TTo> : ISourceDataProviderR<TTo>
    where TFrom : IDataObject
    where TTo : IDataObject
{
    private readonly IDataProviderR<TFrom> _provider;
    private readonly IConfigurationProvider _config;

    public MapDataProviderR(IDataProviderR<TFrom> provider, IConfigurationProvider config)
    {
        _provider = provider;
        _config = config;
    }
    public IQueryable<TTo> Get()
    {
        return _provider.Get().ProjectTo<TTo>(_config);
    }
}