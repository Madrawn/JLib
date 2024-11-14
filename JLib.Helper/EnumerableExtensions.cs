﻿using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace JLib.Helper;

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
    /// removes all <paramref name="elements"/> from the <see cref="list"/>
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
    /// converts the dictionary to a <see cref="ConcurrentDictionary{TKey,TValue}"/>
    /// </summary>
    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary)
        where TKey : notnull
        => new(dictionary);

    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
        this IEnumerable<TValue> col, Func<TValue, TKey> keySelector)
        where TKey : notnull
        => new(col.ToDictionary(keySelector));

    public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TIn, TKey, TValue>(
        this IEnumerable<TIn> col, Func<TIn, TKey> keySelector, Func<TIn, TValue> valueSelector)
        where TKey : notnull
        => new(col.ToDictionary(keySelector, valueSelector));

    public static IEnumerable<ValueTuple<T, int>> AddIndex<T>(this IEnumerable<T> src)
        => src.Select((item, index) => (item, index));
}