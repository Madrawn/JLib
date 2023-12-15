using System.Reflection;
using AutoMapper;
using JLib.Data;
using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static JLib.DataGeneration.DataPackageValues;

namespace JLib.DataGeneration;

public interface IDataPackageRetriever
{
    TPackage GetPackage<TPackage>()
        where TPackage : DataPackage;
}

public interface IDataPackageStore : IDataPackageRetriever
{
    IServiceProvider ServiceProvider { get; }
    /// <summary>
    /// sets the id of the given <paramref name="property"/> to the persisted id or creates a new one<br/>
    /// throws a <see cref="ArgumentOutOfRangeException"/> when the property is neither a <see cref="int"/>, <see cref="Guid"/>, <see cref="IntValueType"/> nor <see cref="GuidValueType"/>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal void SetIdPropertyValue(PropertyInfo property);

    /// <summary>
    /// returns a named id which is not 
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    TId GetNamedId<TId>(IdName name, DataPackage dataPackage);
}

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
public sealed partial class DataPackageManager : IDataPackager, IDataPackageRetriever, IDataPackageStore
{
    /// <summary>
    /// adds the <see cref="DataPackage"/>s configured via <paramref name="includeList"/> and returns a <see cref="IDataPackageRetriever"/> 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="includeList"></param>
    /// <returns></returns>
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
        };
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

    /// <summary>
    /// sets the id of the given <paramref name="property"/> to the persisted id or creates a new one<br/>
    /// throws a <see cref="ArgumentOutOfRangeException"/> when the property is neither a <see cref="int"/>, <see cref="Guid"/>, <see cref="IntValueType"/> nor <see cref="GuidValueType"/>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    void IDataPackageStore.SetIdPropertyValue(PropertyInfo property)
    {
        var packageType = property.ReflectedType
            ?? throw new Exception("Property has no Reflected type");

        var packageInstance = _packageProvider.GetRequiredService(packageType);

        var id = GetId(new(property), property.PropertyType);
        property.SetValue(packageInstance, id);
    }

    /// <summary>
    /// gets the id with the given <paramref name="identifier"/> of the given <paramref name="idType"/><br/>
    /// throws a <see cref="ArgumentOutOfRangeException"/> when the property is neither a <see cref="int"/>, <see cref="Guid"/>, <see cref="IntValueType"/> nor <see cref="GuidValueType"/>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private object GetId(IdIdentifier identifier, Type idType)
    {
        if (idType == typeof(int))
            return _idRegistry.GetIntId(identifier);
        if (idType.IsAssignableTo(typeof(IntValueType)))
        {
            var nativeId = _idRegistry.GetIntId(identifier);
            return _mapper.Map(nativeId, idType);
        }
        if (idType == typeof(Guid))
            return _idRegistry.GetGuidId(identifier);
        if (idType.IsAssignableTo(typeof(GuidValueType)))
        {
            var nativeId = _idRegistry.GetGuidId(identifier);
            return _mapper.Map(nativeId, idType);
        }

        throw new ArgumentOutOfRangeException(nameof(idType), "unknown type");
    }

    /// <summary>
    /// returns a named id which is not 
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    TId IDataPackageStore.GetNamedId<TId>(IdName name, DataPackage dataPackage)
        => GetId(new(new(dataPackage), new("named_" + name.Value)), typeof(TId)).CastTo<TId>();

    IServiceProvider IDataPackageStore.ServiceProvider
        => _dataServiceProvider;
}