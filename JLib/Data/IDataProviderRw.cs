using System.Linq.Expressions;
using JLib.AutoMapper;
using JLib.Helper;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib.Data;

public sealed class IgnoredDataObject : IDataObject
{
    private IgnoredDataObject()
    {

    }

    public Guid Id { get; }
}

public interface IDataObject
{
    public Guid Id { get; }

}

/// <summary>
/// enables a class to be requested and edited via <see cref="IDataProviderRw{TData}"/> 
/// </summary>
public interface IEntity : IDataObject
{
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

public interface IGraphQlDataObject : IDataObject
{

}

public interface IDataProviderR<TData>
    where TData : IDataObject
{
    public IQueryable<TData> Get();
}

public interface ISourceDataProviderR<TData> : IDataProviderR<TData>
    where TData : IDataObject
{

}

public interface IDataProviderRw<TData> : IDataProviderR<TData>
    where TData : IEntity
{
    public void Add(TData item);
    public void Add(IEnumerable<TData> items);
    public void Remove(Guid itemId);
    public void Remove(IEnumerable<Guid> itemIds);
}

public interface ISourceDataProviderRw<TData> : IDataProviderRw<TData>, ISourceDataProviderR<TData>
    where TData : IEntity
{

}
