using JLib.AutoMapper;
using JLib.Helper;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib.Data;

/// <summary>
/// enables a class to be requested and edited via <see cref="IDataProviderRw{TData}"/> 
/// </summary>
public interface IEntity
{
    public Guid Id { get; }
}

/// <summary>
/// marks an entity as being the primary domain representation using value types etc for the command side of the CQRS app.
/// </summary>
public interface ICommandEntity : IEntity
{

}

public interface IMappedCommandEntity<TEntity> : ICommandEntity
    where TEntity : IEntity
{ }
public interface IMappedGraphQlDataObject<TEntity> : IGraphQlDataObject
    where TEntity : IEntity
{ }

public interface IGraphQlDataObject
{

}
public interface IGraphQlDataObject<TEntity> : IGraphQlDataObject
    where TEntity : IEntity
{

}


public interface IDataProviderR<TData>
{
    public IQueryable<TData> Get();

}
public interface IDataProviderRw<TData> : IDataProviderR<TData>
    where TData : IEntity
{
    public void Add(TData item);
    public void Add(IEnumerable<TData> items);
    public void Remove(Guid itemId);
    public void Remove(IEnumerable<Guid> itemIds);
}
