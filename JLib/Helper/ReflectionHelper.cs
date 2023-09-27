using System.Collections.ObjectModel;
using System.Reflection;

namespace JLib.Helper;

public record TypeAttributeInfo(Type Type, Attribute Attribute);
public static class ReflectionHelper
{
    public static bool HasAnyCustomAttribute<T>(this Type type, bool inherit = true)
        where T : Attribute
    {
        var compare = type.TryGetGenericTypeDefinition();
        return Attribute.GetCustomAttributes(type, inherit).Any(t => t.GetType().TryGetGenericTypeDefinition() == compare);
    }

    public static bool HasCustomAttribute(this MemberInfo type, Type attributeType, bool inherit = true)
        => type.GetCustomAttribute(attributeType, inherit) is not null;
    public static bool HasCustomAttribute<T>(this MemberInfo type, bool inherit = true)
        where T : Attribute =>
        type.HasCustomAttribute(typeof(T), inherit);
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

    #region get property nullabillity
    // source: https://stackoverflow.com/questions/58453972/how-to-use-net-reflection-to-check-for-nullable-reference-type
    public static bool IsNullable(this PropertyInfo property) =>
    IsNullableHelper(property.PropertyType, property.DeclaringType, property.CustomAttributes);

    public static bool IsNullable(this FieldInfo field) =>
        IsNullableHelper(field.FieldType, field.DeclaringType, field.CustomAttributes);

    public static bool IsNullable(this ParameterInfo parameter) =>
        IsNullableHelper(parameter.ParameterType, parameter.Member, parameter.CustomAttributes);

    private static bool IsNullableHelper(Type memberType, MemberInfo? declaringType, IEnumerable<CustomAttributeData> customAttributes)
    {
        if (memberType.IsValueType)
            return Nullable.GetUnderlyingType(memberType) != null;

        var nullable = customAttributes
            .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        if (nullable != null && nullable.ConstructorArguments.Count == 1)
        {
            var attributeArgument = nullable.ConstructorArguments[0];
            if (attributeArgument.ArgumentType == typeof(byte[]))
            {
                var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value!;
                if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
                {
                    return (byte)args[0].Value! == 2;
                }
            }
            else if (attributeArgument.ArgumentType == typeof(byte))
            {
                return (byte)attributeArgument.Value! == 2;
            }
        }

        for (var type = declaringType; type != null; type = type.DeclaringType)
        {
            var context = type.CustomAttributes
                .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            if (context != null &&
                context.ConstructorArguments.Count == 1 &&
                context.ConstructorArguments[0].ArgumentType == typeof(byte))
            {
                return (byte)context.ConstructorArguments[0].Value! == 2;
            }
        }

        // Couldn't find a suitable attribute
        return false;
    }
    #endregion

}