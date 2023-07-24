using System.Collections.Immutable;

namespace JLib.Helper;

public static class EnumerableHelper
{
    /// <summary>
    ///     removes all null entries from the list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="src"></param>
    /// <returns></returns>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> src)
        => src.Where(x => x is not null)!;

    public static bool None<T>(this IEnumerable<T> src, Func<T, bool>? selector = null)
        => selector is null
            ? !src.Any()
            : !src.Any(selector);

    public static IImmutableDictionary<TKey, IEnumerable<TValue>> ToImmutableDictionary<TKey, TValue>(
        this IEnumerable<IGrouping<TKey, TValue>> groups)
        where TKey : notnull
        => groups.ToImmutableDictionary(kv => kv.Key, kv => kv.AsEnumerable());

    public static IGrouping<TKey, TValue> ByKey<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> col, TKey key)
        => col.First(x => x.Key?.Equals(key) ?? key is null);

    public static IEnumerable<T> Except<T>(this IEnumerable<T> col, params T[] values)
        => col.Except(values.AsEnumerable());
    public static void RemoveWhere<T>(this ICollection<T> col, Func<T, bool> filter)
    {
        foreach (var i in col.Where(filter).ToArray())
            col.Remove(i);
    }
    public static void RemoveWhere<T>(this IList<T> col, Func<T, bool> filter)
    {
        foreach (var i in col.Where(filter).ToArray())
            col.Remove(i);
    }
    public static void Remove<T>(this IList<T> col, IEnumerable<T> toRemove)
    {
        foreach (var i in toRemove)
            col.Remove(i);
    }
}