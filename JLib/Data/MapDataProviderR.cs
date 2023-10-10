using System.ComponentModel.Design;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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