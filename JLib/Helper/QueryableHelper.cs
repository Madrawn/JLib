using JLib.Data;
using JLib.ValueTypes;

namespace JLib.Helper;
public static class QueryableHelper
{
    public static T ById<T>(this IQueryable<T> query, Guid id)
        where T : IDataObject 
        => query.Single(e => e.Id == id);

    public static T ById<T, TId>(this IQueryable<T> query, TId id)
        where T : ITypedIdDataObject<TId>
        where TId : GuidValueType
        => query.ById<T>(id.Value);
}
