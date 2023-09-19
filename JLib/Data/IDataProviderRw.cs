using JLib.AutoMapper;

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

/// <summary>
/// marks the class as parameter for a GraphQl mutation and creates a map from the parameter to the given entity (using <see cref="EntityProfile"/>)
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IGraphQlMutationParameter<TEntity>
    where TEntity : IEntity
{

}
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
