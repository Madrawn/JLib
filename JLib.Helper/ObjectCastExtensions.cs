namespace JLib.Helper;

/// <summary>
/// contains extension methods for casting <see cref="object"/>
/// </summary>
public static class ObjectCastExtensions
{
    /// <returns>(<typeparamref name="T"/>) <paramref name="obj"/></returns>
    public static T CastTo<T>(this object obj)
        => (T)obj;

    /// <returns><paramref name="obj"/> <see langword="as"/> <typeparamref name="T"/></returns>
    public static T? As<T>(this object obj)
        where T : class?
        => obj as T;
}