using System.Reflection;
using AutoMapper;
using JLib.Data;
using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JLib.DataGeneration;

public interface IDataPackager
{
    IDataPackager Include<TPackage>()
        where TPackage : DataPackage;

    IDataPackager Include<TPackage1, TPackage2>()
        where TPackage1 : DataPackage
        where TPackage2 : DataPackage
        => Include<TPackage1>().Include<TPackage2>();
    IDataPackager Include<TPackage1, TPackage2, TPackage3>()
        where TPackage1 : DataPackage
        where TPackage2 : DataPackage
        where TPackage3 : DataPackage
        => Include<TPackage1, TPackage2>().Include<TPackage3>();
    IDataPackager Include<TPackage1, TPackage2, TPackage3, TPackage4>()
        where TPackage1 : DataPackage
        where TPackage2 : DataPackage
        where TPackage3 : DataPackage
        where TPackage4 : DataPackage
        => Include<TPackage1, TPackage2>().Include<TPackage3, TPackage4>();
    IDataPackager Include<TPackage1, TPackage2, TPackage3, TPackage4, TPackage5>()
        where TPackage1 : DataPackage
        where TPackage2 : DataPackage
        where TPackage3 : DataPackage
        where TPackage4 : DataPackage
        where TPackage5 : DataPackage
        => Include<TPackage1, TPackage2, TPackage3>().Include<TPackage4, TPackage5>();
}

/// <summary>
/// adds mock data to the application
/// <br/>adds data to te <see cref="IDataProviderRw{TData}"/> using the <see cref="Include{TPackage}"/> method which take <see cref="DataGeneration.DataPackage"/> as parameter
/// </summary>
public sealed partial class DataPackageManager : IDataPackager, IDataPackageRetriever
{
    public static IDataPackageRetriever ApplyPackages(IServiceProvider serviceProvider,
        Action<IDataPackager> includeList)
    {
        var context = new DataPackageManager(serviceProvider);
        includeList(context);
        context._idRegistry.SaveToFile();
        return context;
    }

    private readonly IServiceProvider _dataServiceProvider;
    private readonly IServiceProvider _packageProvider;
    private readonly IMapper _mapper;
    private readonly IdRegistry _idRegistry = new();

    private DataPackageManager(IServiceProvider dataServices)
    {
        _dataServiceProvider = dataServices;
        _mapper = dataServices.GetRequiredService<IMapper>();
        var packageServices = new ServiceCollection
        {
            dataServices.GetRequiredService<ITypeCache>().All<DataPackageType>()
                .Select(dp => new ServiceDescriptor(dp.Value, dp.Value, ServiceLifetime.Singleton))
        // todo: should not be transient4e3
        }.AddTransient<IDataPackageStore, DataPackageStore>();
        _packageProvider = packageServices.BuildServiceProvider();
    }

    public IDataPackager Include<TPackage>()
        where TPackage : DataGeneration.DataPackage
    {
        _packageProvider.GetRequiredService<TPackage>();
        return this;
    }

    public TPackage GetPackage<TPackage>() where TPackage : DataGeneration.DataPackage
        => _packageProvider.GetRequiredService<TPackage>();
}