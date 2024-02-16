using JLib.DataProvider;
using JLib.ValueTypes;

namespace JLib.Cqrs;
public static class QueryableByTypedIdExtensions
{
    public static T ById<T, TId>(this IQueryable<T> query, TId id)
        where T : ITypedIdDataObject<TId>
        where TId : GuidValueType
        => query.ById<T>(id.Value);
}
