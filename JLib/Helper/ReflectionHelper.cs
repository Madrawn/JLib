using System.Reflection;

namespace JLib.Helper;

public record TypeAttributeInfo(Type Type, Attribute Attribute);
public static class ReflectionHelper
{
    public static IEnumerable<TypeAttributeInfo> GetTypesWithAttribute<TAttribute>(this Assembly assembly)
        where TAttribute : Attribute =>
        assembly
            .GetTypes()
            .Select(type =>
            {
                var attribute = type.GetCustomAttribute<TAttribute>();
                return attribute is null ? null : new TypeAttributeInfo(type, attribute);
            }).WhereNotNull();

    public static bool HasCustomAttribute<T>(this MemberInfo type, bool inherit = true)
        where T : Attribute =>
        type.GetCustomAttributes(inherit).Any(x => x is T);
    public static T[] GetCustomAttributes<T>(this MemberInfo type, bool inherit = true)
        where T : Attribute =>
        Attribute.GetCustomAttributes(type, typeof(T), inherit).Cast<T>().ToArray();
    public static Attribute[] GetCustomAttributes(this MemberInfo type, Type attributeType, bool inherit = true)
        => Attribute.GetCustomAttributes(type, attributeType, inherit).ToArray();

    public static Type? TryGetGenericTypeDefinition(this Type type)
        => type.IsGenericType ? type.GetGenericTypeDefinition() : null;

    public static IEnumerable<T> WithAttribute<T, TAttribute>(this IEnumerable<T> src)
        where T : MemberInfo
        where TAttribute : Attribute
        => src.Where(m => m.HasCustomAttribute<TAttribute>());
}