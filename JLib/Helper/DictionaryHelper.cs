using System.Collections.Concurrent;

namespace JLib.Helper;

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
    public static TValue GetValueOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory)
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
}
