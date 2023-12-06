using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace JLib.Helper;

public static class EnumerableHelper
{
    /// <summary>
    /// returns true if <paramref name="src"/> contains at least 2 elements without enumerating the entire list
    /// </summary>
    public static bool Multiple<T>(this IEnumerable<T> src)
    {
        if (src is IReadOnlyCollection<T> col)
            return col.Count > 1;
        using var e = src.GetEnumerator();
        e.MoveNext();//returns true if count >=1
        return e.MoveNext();// returns true if count >=2
    }

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

    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
        this IEnumerable<TValue> col, Func<TValue, TKey> keySelector)
        where TKey : notnull
        => new(col.ToDictionary(keySelector));

    public static IEnumerable<ValueTuple<T,int>> AddIndex<T>(this IEnumerable<T> src)
        => src.Select((item, index) => (item, index));
}