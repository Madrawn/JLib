using JLib.ValueTypes;

namespace JLib.DataProvider;

public static class QueryableHelper
{
    public static T ById<T>(this IQueryable<T> query, Guid id)
        where T : IDataObject
        => query.Single(e => e.Id == id);

}