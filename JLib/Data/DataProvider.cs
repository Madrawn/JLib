namespace JLib.Data;


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
