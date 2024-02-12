namespace JLib.DataProvider;

public interface IDataProviderRw<TData> : IDataProviderR<TData>
    where TData : IEntity
{
    public void Add(TData item);
    public void Add(IReadOnlyCollection<TData> items);
    public void Remove(Guid itemId);
    public void Remove(TData item);
    public void Remove(IReadOnlyCollection<Guid> itemIds);
    public void Remove(IReadOnlyCollection<TData> items);
}