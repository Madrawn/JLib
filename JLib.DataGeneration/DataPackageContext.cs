using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using AutoMapper;
using JLib.Data;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JLib.DataGeneration;

public interface IDataPackageStore
{
    TEntity[] AddEntities<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : IEntity;
    TPackage GetPackage<TPackage>()
        where TPackage : DataPackage;
    internal GuidValueType RetrieveId(PropertyInfo property);
    TId? DeriveId<TId>(GuidValueType? id, Type dataPackage)
        where TId : GuidValueType;

    TId? DeriveId<TId>(GuidValueType? idN, GuidValueType? idM, Type dataPackage)
        where TId : GuidValueType;
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
/// <br/>adds data to te <see cref="IDataProviderRw{TData}"/> using the <see cref="Include{TPackage}"/> method which take <see cref="DataPackage"/> as parameter
/// </summary>
public sealed class DataPackageManager : IDataPackager, IDataPackageStore
{
    public static void ApplyPackages(IServiceProvider serviceProvider, Action<IDataPackager> includeList)
    {
        var context = new DataPackageManager(serviceProvider);
        includeList(context);
        context._idRegistry.SaveToFile();
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
                .Select(dp => new ServiceDescriptor(dp.Value,dp.Value, ServiceLifetime.Singleton))
        }.AddSingleton<IDataPackageStore>(this);
        _packageProvider = packageServices.BuildServiceProvider();
    }

    public IDataPackager Include<TPackage>()
        where TPackage : DataPackage
    {
        _packageProvider.GetRequiredService<TPackage>();
        return this;
    }
    public TEntity[] AddEntities<TEntity>(IEnumerable<TEntity> entities) where TEntity : IEntity
    {
        var e = entities.ToArray();
        _dataServiceProvider.GetRequiredService<IDataProviderRw<TEntity>>().Add(e);
        return e;
    }

    public TPackage GetPackage<TPackage>() where TPackage : DataPackage => _packageProvider.GetRequiredService<TPackage>();

    private Guid GetModifier<TId>(Type dataPackage)
        => _idRegistry.GetId(dataPackage.FullClassName(), $"modifier_{typeof(TId).FullClassName()}");

    private static Guid CombineGuid(Guid id1, Guid id2)
    {
        var binary = id1.ToByteArray().Zip(id2.ToByteArray())
            .Select(x => (byte)(x.First ^ x.Second)).ToArray();
        var guid = new Guid(binary);
        return guid;
    }

    /// <summary>
    /// works by taking a guid which is generated for each <see cref="Type"/>+<see cref="GuidValueType"/> of the related entity and applying a bitwise xor between it and the given id.
    /// <br/> this should result in a new, unique and deterministic id per entity relation 
    /// </summary>
    public TId? DeriveId<TId>(GuidValueType? id, Type dataPackage)
        where TId : GuidValueType
    {
        if (id is null)
            return null;
        return _mapper.Map<TId?>(CombineGuid(id.Value, GetModifier<TId>(dataPackage)));
    }

    public TId? DeriveId<TId>(GuidValueType? idN, GuidValueType? idM, Type dataPackage)
        where TId : GuidValueType
    {
        if (idN is null && idM is null)
            return null;
        if (idN is not null && idM is null)
            return DeriveId<TId>(idN, dataPackage);
        if (idN is null && idM is not null)
            return DeriveId<TId>(idM, dataPackage);
        var stage1 = CombineGuid(idN!.Value, idM!.Value);
        var stage2 = CombineGuid(stage1, GetModifier<TId>(dataPackage));
        return _mapper.Map<TId?>(stage2);
    }

    GuidValueType IDataPackageStore.RetrieveId(PropertyInfo property)
    {
        if (property.ReflectedType is null)
            throw new Exception("reflected type is null");
        var id = _idRegistry.GetId(property.ReflectedType.FullClassName(), property.Name);
        return _mapper.Map(id, typeof(Guid), property.PropertyType) as GuidValueType ?? throw new("not mapped to a guidVt");
    }
}