using JLib.Data;
using Microsoft.EntityFrameworkCore;

namespace JLib.EfCore;

public class EfCoreDataProvider<TEntity> : ISourceDataProviderRw<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbContext _dbContext;

    public EfCoreDataProvider(DbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public IQueryable<TEntity> Get()
        => _dbContext.Set<TEntity>();

    public void Add(TEntity item)
        => _dbContext.Set<TEntity>().Add(item);

    public void Add(IEnumerable<TEntity> items)
        => _dbContext.Set<TEntity>().AddRange(items);

    public void Remove(Guid itemId)
    {
        var set = _dbContext.Set<TEntity>();
        var item = set.Single(x => x.Id == itemId);
        _dbContext.Set<TEntity>().Remove(item);
    }

    public void Remove(IEnumerable<Guid> itemIds)
    {
        var set = _dbContext.Set<TEntity>();
        var items = set.Where(x => itemIds.Contains(x.Id));
        _dbContext.Set<TEntity>().RemoveRange(items);
    }
}