namespace JLib.Data;

public interface IEntity
{
    public Guid Id { get; }
}

public interface IGraphQlMutationParameter
{

}
public interface IGraphQlMutationParameter<TEntity> : IGraphQlMutationParameter
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
