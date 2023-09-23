using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;
using Serilog;

namespace JLib.Data;

public class MockDataProvider<TEntity> : IDataProviderRw<TEntity>
    where TEntity : IEntity
{
    private readonly PropertyInfo _idProperty;

    private readonly ConstructorInfo _idPropertyVtCtor;

    public MockDataProvider()
    {
        _idProperty = typeof(TEntity)
            .GetProperties()
            .Single(x => x.Name == nameof(IEntity.Id) && x.PropertyType.IsAssignableTo<GuidValueType>());
        _idPropertyVtCtor = _idProperty.PropertyType.GetConstructor(new[] { typeof(Guid) }) ?? throw new InvalidSetupException("vtId ctor not found");
        Log.Verbose("creating {type}",GetType().FullClassName());
    }
    private void AddId(TEntity entity)
    {
        if (_idProperty.GetValue(entity) is not null)
            return;
        _idProperty.SetValue(entity, _idPropertyVtCtor.Invoke(new object[] { Guid.NewGuid() }));
    }
    private readonly List<TEntity> _items = new();
    public IQueryable<TEntity> Get()
    {
        Log.Verbose("MockDataProvider Query for {0}",typeof(TEntity).Name);
        return _items.AsQueryable();
    }

    public void Add(TEntity item)
    {
        _items.Add(item);
        AddId(item);
    }

    public void Add(IEnumerable<TEntity> items)
    {
        var newEntities = items.ToArray();
        _items.AddRange(newEntities);
        foreach (var entity in newEntities)
            AddId(entity);
    }

    public void Remove(Guid itemId) => _items.RemoveWhere(i => i.Id == itemId);

    public void Remove(IEnumerable<Guid> itemIds) => _items.RemoveWhere(i => itemIds.Contains(i.Id));
}