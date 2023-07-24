namespace JLib.Helper;

public static class DictionaryHelper
{
    /// <summary>
    /// returns the value of the given key. if the key is not found, a new value will be generated and added
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <param name="generator"></param>
    /// <returns></returns>
    public static TValue GetValueOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key,
        Func<TValue> generator)
        => dict.GetValueOrAdd(key, _ => generator());
    public static TValue GetValueOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> generator)
    {
        if (dict.TryGetValue(key, out var value))
            return value;

        value = generator(key);
        dict.Add(key, value);

        return value;
    }
}
