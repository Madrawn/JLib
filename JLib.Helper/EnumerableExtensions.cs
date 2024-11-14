using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace JLib.Helper;

/// <summary>
/// contains extension methods for <see cref="IEnumerable{T}"/>
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// returns true if <paramref name="src"/> contains at least 2 elements without enumerating the entire list
    /// </summary>
    public static bool Multiple<T>(this IEnumerable<T> src)
    {
        if (src is IReadOnlyCollection<T> col)
            return col.Count > 1;
        using var e = src.GetEnumerator();
        e.MoveNext(); //returns true if count >=1
        return e.MoveNext(); // returns true if count >=2
    }

    /// <summary>
    ///     removes all null entries from the list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="src"></param>
    /// <returns></returns>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> src)
        => src.Where(x => x is not null)!;

    /// <returns>true if the <paramref name="enumerable"/> contains no elements which satisfy the given <paramref name="selector"/> or no elements at all if it is nulll.</returns>
    public static bool None<T>(this IEnumerable<T> enumerable, Func<T, bool>? selector = null)
        => selector is null
            ? !enumerable.Any()
            : !enumerable.Any(selector);

    /// <summary>
    /// converts the given <paramref name="groups"/> to an <see cref="IImmutableDictionary{TKey,TValue}"/>
    /// </summary>
    public static IImmutableDictionary<TKey, IEnumerable<TValue>> ToImmutableDictionary<TKey, TValue>(
        this IEnumerable<IGrouping<TKey, TValue>> groups)
        where TKey : notnull
        => groups.ToImmutableDictionary(kv => kv.Key, kv => kv.AsEnumerable());


    /// <returns>the first element of the <paramref name="grouping"/> which has the given <paramref name="key"/></returns>
    public static IGrouping<TKey, TValue> ByKey<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> grouping, TKey key)
        => grouping.First(x => x.Key?.Equals(key) ?? key is null);

    /// <summary>
    /// <inheritdoc cref="Enumerable.Except{TSource}(IEnumerable{TSource},IEnumerable{TSource})"/>
    /// </summary>
    public static IEnumerable<T> Except<T>(this IEnumerable<T> col, params T[] values)
        => col.Except(values.AsEnumerable());

    /// <summary>
    /// removes all elements from the <paramref name="collection"/> which match the <paramref name="filter"/>
    /// </summary>
    public static void RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> filter)
    {
        foreach (var i in collection.Where(filter).ToArray())
            collection.Remove(i);
    }

    /// <summary>
    /// removes all elements from the <paramref name="list"/> which match the <paramref name="filter"/>
    /// </summary>
    public static void RemoveWhere<T>(this IList<T> list, Func<T, bool> filter)
    {
        foreach (var i in list.Where(filter).ToArray())
            list.Remove(i);
    }

    /// <summary>
    /// removes all <paramref name="elements"/> from the <paramref name="list"/>
    /// </summary>
    public static void Remove<T>(this IList<T> list, IEnumerable<T> elements)
    {
        foreach (var i in elements)
            list.Remove(i);
    }

    /// <summary>
    /// casts the <paramref name="enumerable"/> as <see cref="IReadOnlyCollection{T}"/>, materializing it and preventing all write operations
    /// </summary>
    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        => enumerable.ToArray();

    /// <summary>
    /// converts the <paramref name="dictionary"/> to a <see cref="ConcurrentDictionary{TKey,TValue}"/>
    /// </summary>
    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary)
        where TKey : notnull
        => new(dictionary);

    /// <summary>
    /// converts the <paramref name="enumerable"/> to a <see cref="ConcurrentDictionary{TKey,TValue}"/> using the given <paramref name="keySelector"/>
    /// </summary>
    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
        this IEnumerable<TValue> enumerable, Func<TValue, TKey> keySelector)
        where TKey : notnull
        => new(enumerable.ToDictionary(keySelector));

    /// <summary>
    /// converts the <paramref name="enumerable"/> to a <see cref="ConcurrentDictionary{TKey,TValue}"/> using the given <paramref name="keySelector"/> and <paramref name="valueSelector"/>
    /// </summary>
    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TIn, TKey, TValue>(
        this IEnumerable<TIn> enumerable, Func<TIn, TKey> keySelector, Func<TIn, TValue> valueSelector)
        where TKey : notnull
        => new(enumerable.ToDictionary(keySelector, valueSelector));

    /// <summary>
    /// Adds the index to each entry in the <paramref name="enumerable"/>
    /// </summary>
    public static IEnumerable<ValueTuple<T, int>> AddIndex<T>(this IEnumerable<T> enumerable)
        => enumerable.Select((item, index) => (item, index));
}