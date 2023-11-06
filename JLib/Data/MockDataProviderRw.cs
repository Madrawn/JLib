using System.Reflection;
using JLib.Data.Authorization;
using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;
using Serilog;

namespace JLib.Data;

public class MockDataProvider<TEntity> : ISourceDataProviderRw<TEntity>
    where TEntity : class, IEntity
{
    private readonly PropertyInfo _idProperty;

    private Func<Guid, object> _idGenerator;

    public MockDataProvider(IAuthorizationInfo<TEntity> authorizationInfo)
    {
        _authorize = authorizationInfo;
        // reflection to support vt ids
        _idProperty = typeof(TEntity)
            .GetProperties()
            .Single(x => x.Name == nameof(IEntity.Id));
        if (_idProperty.PropertyType.IsAssignableTo<GuidValueType>())
        {
            var idPropertyVtCtor = _idProperty.PropertyType.GetConstructor(new[] { typeof(Guid) }) ??
                                throw new InvalidSetupException("vtId ctor not found");
            _idGenerator = id => idPropertyVtCtor.Invoke(new object[] { id }).CastTo<GuidValueType>();
        }
        else
            _idGenerator = id => id;
        Log.Verbose("creating {type}", GetType().FullClassName());
    }
    private void AddId(TEntity entity)
    {
        if (_idProperty.GetValue(entity) is not null)
            return;
        _idProperty.SetValue(entity, _idGenerator?.Invoke(Guid.NewGuid()));
    }
    private readonly List<TEntity> _items = new();
    private readonly IAuthorizationInfo<TEntity> _authorize;

    public IQueryable<TEntity> Get()
    {
        Log.Verbose("MockDataProvider Expression for {0}", typeof(TEntity).Name);
        return _items.AsQueryable().Where(_authorize.Expression());
    }

    public void Add(TEntity item)
    {
        _authorize.AndRaiseException(item);
        _items.Add(item);
        AddId(item);
    }

    public void Add(IReadOnlyCollection<TEntity> items)
    {
        var itemArray = items.ToArray();
        _authorize.AndRaiseException(itemArray);
        var newEntities = itemArray;
        _items.AddRange(newEntities);
        foreach (var entity in newEntities)
            AddId(entity);
    }

    public void Remove(Guid itemId)
    {
        var item = this.CastTo<IDataProviderR<TEntity>>().Get(itemId);
        _items.Remove(item);
    }

    public void Remove(TEntity item) => _items.Remove(item);

    public void Remove(IReadOnlyCollection<Guid> itemIds)
    {
        var items = this.CastTo<IDataProviderR<TEntity>>().Get(itemIds).Select(x => x.Value);
        _items.Remove(items);
    }

    public void Remove(IReadOnlyCollection<TEntity> items) 
        => _items.Remove(items);
}