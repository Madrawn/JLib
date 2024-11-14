using System.Collections.Concurrent;

namespace JLib.Helper;

/// <summary>
/// contains extension methods for <see cref="IDictionary{TKey,TValue}"/>
/// </summary>
public static class DictionaryHelper
{
    /// <summary>
    /// returns the value of the given key. if the key is not found, a new value will be generated and added
    /// </summary>
    public static TValue GetValueOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
        Func<TValue> valueFactory)
        where TKey : notnull
        => dict.GetValueOrAdd(key, _ => valueFactory());

    /// <summary>
    /// <inheritdoc cref="GetValueOrAdd{TKey,TValue}(IDictionary{TKey,TValue},TKey,Func{TValue})"/>
    /// </summary>
    public static TValue GetValueOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
        Func<TKey, TValue> valueFactory)
        where TKey : notnull
    {
        if (dict is ConcurrentDictionary<TKey, TValue> cDict)
            return cDict.GetOrAdd(key, valueFactory);

        if (dict.TryGetValue(key, out var value))
            return value;

        value = valueFactory(key);
        dict.Add(key, value);

        return value;
    }

    /// <summary>
    /// adds or overwrites the value of <paramref name="key"/> with <paramref name="value"/>
    /// </summary>
    public static void AddOrReplace<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue value)
        where TKey : notnull
        => dict.AddOrUpdate(key, _ => value, (_, _) => value);

    /// <summary>
    /// returns the value of the given <paramref name="key"/>. if the <paramref name="key"/> is not found, the <see langword="default"/> value will be returned.
    /// </summary>
    public static TValue? TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
    {
        return dict.TryGetValue(key, out var value)
            ? value
            : default;
    }
}