using System.ComponentModel.Design;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace JLib.Data;

public class MapDataProvider<TFrom, TTo> : ISourceDataProviderR<TTo>
    where TFrom : IDataObject
    where TTo : IDataObject
{
    private readonly IDataProviderR<TFrom> _provider;
    private readonly IConfigurationProvider _config;

    public MapDataProvider(IDataProviderR<TFrom> provider, IConfigurationProvider config)
    {
        _provider = provider;
        _config = config;
        Log.Verbose("creating {type}", GetType().FullClassName());
    }
    public IQueryable<TTo> Get()
    {
        return _provider.Get().ProjectTo<TTo>(_config);
    }
}