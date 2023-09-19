using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace JLib.Data;

public class WritableMapDataProvider<TFrom, TTo> : IDataProviderRw<TTo>
    where TFrom : IEntity
    where TTo : IEntity
{
    private readonly IDataProviderRw<TFrom> _provider;
    private readonly IConfigurationProvider _config;
    private readonly IMapper _mapper;

    public WritableMapDataProvider(IDataProviderRw<TFrom> provider, IConfigurationProvider config, IMapper mapper)
    {
        _provider = provider;
        _config = config;
        _mapper = mapper;
    }
    public IQueryable<TTo> Get()
        => _provider.Get().ProjectTo<TTo>(_config);

    public void Add(TTo item) 
        => _provider.Add(_mapper.Map<TFrom>(item));

    public void Add(IEnumerable<TTo> items) 
        => _provider.Add(_mapper.Map<TFrom[]>(items));

    public void Remove(Guid itemId) 
        => _provider.Remove(itemId);

    public void Remove(IEnumerable<Guid> itemIds) 
        => _provider.Remove(itemIds);
}