using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace JLib.Data;

public class MockDataProvider<TEntity> : IDataProviderRw<TEntity>, IDisposable
    where TEntity : IEntity
{
    public MockDataProvider()
    {
        
    }
    private readonly List<TEntity> _items = new();
    public IQueryable<TEntity> Get()
        => _items.AsQueryable();

    public void Add(TEntity item)
        => _items.Add(item);

    public void Add(IEnumerable<TEntity> items) => _items.AddRange(items);

    public void Dispose()
    {

    }
}

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