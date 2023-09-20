using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JLib.Data;
using JLib.Helper;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib.DataGeneration;

[IsDerivedFrom<DataPackage>, NotAbstract]
public record DataPackageType(Type Value) : TypeValueType(Value);

public abstract class DataPackage
{
    readonly IDataPackageStore _dataPackages;
    protected DataPackage(IDataPackageStore dataPackages)
    {
        _dataPackages = dataPackages;
        foreach (var propertyInfo in GetType().GetProperties())
        {
            if (!propertyInfo.PropertyType.IsAssignableTo<GuidValueType>())
                continue;
            if (propertyInfo.GetMethod?.IsPublic is not true)
                continue;
            if (propertyInfo.CanWrite is false)
                throw new(propertyInfo.DeclaringType?.FullClassName() + "." + propertyInfo.Name +
                          " can not be written");
            if (propertyInfo.SetMethod?.IsPublic is true)
                throw new(propertyInfo.DeclaringType?.FullClassName() + "." + propertyInfo.Name +
                          " set method must be protected");
            var id = _dataPackages.RetrieveId(propertyInfo);
            propertyInfo.SetValue(this, id);
        }
    }

    protected TEntity[] AddEntities<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : IEntity
        => _dataPackages.AddEntities(entities);


    protected TId? DeriveId<TId>(GuidValueType? id)
        where TId : GuidValueType
        => _dataPackages.DeriveId<TId>(id, GetType());

    protected TId? DeriveId<TId>(GuidValueType? idN, GuidValueType? idM)
        where TId : GuidValueType
        => _dataPackages.DeriveId<TId>(idN, idM, GetType());
}
