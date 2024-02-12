namespace JLib.DataProvider;

public interface IDataProviderR<TDataObject>
    where TDataObject : IDataObject
{
    public IQueryable<TDataObject> Get();

    public TDataObject Get(Guid id);

    public IReadOnlyDictionary<Guid, TDataObject> Get(IReadOnlyCollection<Guid> ids);

    public TDataObject? TryGet(Guid? id);
    public bool Contains(Guid? id);
}