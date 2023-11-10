using JLib.Data;
using JLib.Data.Authorization;
using Microsoft.EntityFrameworkCore;

namespace JLib.EfCore;

public class EfCoreDataProviderR<TEntity> : DataProviderRBase<TEntity>, ISourceDataProviderR<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbContext _dbContext;
    private readonly IAuthorizationInfo<TEntity> _authorize;

    public EfCoreDataProviderR(DbContext dbContext, IAuthorizationInfo<TEntity> authorize)
    {
        _dbContext = dbContext;
        _authorize = authorize;
    }

    public override IQueryable<TEntity> Get() => _dbContext.Set<TEntity>().Where(_authorize.Expression()).AsNoTracking();
}

public class EfCoreDataProviderRw<TEntity> : DataProviderRBase<TEntity>, ISourceDataProviderRw<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbContext _dbContext;
    private readonly IAuthorizationInfo<TEntity> _authorize;

    public EfCoreDataProviderRw(DbContext dbContext, IAuthorizationInfo<TEntity> authorize)
    {
        _dbContext = dbContext;
        _authorize = authorize;
    }
    public override IQueryable<TEntity> Get()
        => _dbContext.Set<TEntity>().Where(_authorize.Expression());

    public void Add(TEntity item)
        => _dbContext.Set<TEntity>().Add(_authorize.AndRaiseException(item));

    public void Add(IReadOnlyCollection<TEntity> items)
        => _dbContext.Set<TEntity>().AddRange(_authorize.AndRaiseException(items));

    public void Remove(Guid itemId)
    {
        var set = _dbContext.Set<TEntity>();
        var item = set.Single(x => x.Id == itemId);
        _authorize.AndRaiseException(item);
        _dbContext.Set<TEntity>().Remove(item);
    }

    public void Remove(TEntity item)
        => _dbContext.Remove(item);

    public void Remove(IReadOnlyCollection<Guid> itemIds)
    {
        var set = _dbContext.Set<TEntity>();
        var items = set.Where(x => itemIds.Contains(x.Id)).ToArray();
        _authorize.AndRaiseException(items);
        _dbContext.Set<TEntity>().RemoveRange(items);
    }

    public void Remove(IReadOnlyCollection<TEntity> items)
        => _dbContext.RemoveRange(items);
}