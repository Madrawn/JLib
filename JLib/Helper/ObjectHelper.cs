namespace JLib.Helper;
public static class ObjectHelper
{
    public static T CastTo<T>(this object obj)
        => (T)obj;
    public static T? As<T>(this object obj)
        where T : class?
        => obj as T;
}
