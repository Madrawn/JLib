using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace JLib.Helper;

public record TypeAttributeInfo(Type Type, Attribute Attribute);

public static class ReflectionHelper
{
    // https://alistairevans.co.uk/2020/11/01/detecting-init-only-properties-with-reflection-in-c-9/
    /// <summary>
    /// checks whether the property is init<br/>
    /// { get; } => false<br/>
    /// { get; set; } => false;<br/>
    /// { get; init; } => true;
    /// </summary>
    public static bool IsInit(this PropertyInfo property)
    {
        if (!property.CanWrite)
        {
            return false;
        }

        var setMethod = property.SetMethod;

        // Get the modifiers applied to the return parameter.
        var setMethodReturnParameterModifiers = setMethod?.ReturnParameter.GetRequiredCustomModifiers();

        // Init-only properties are marked with the IsExternalInit type.
        return setMethodReturnParameterModifiers?.Contains(typeof(IsExternalInit)) ?? false;
    }
    /// <summary>
    /// Checks, whether the given <paramref name="type"/> is decorated with the given <paramref name="attributeType"/>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="attributeType"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
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

    public static Type TryGetGenericTypeDefinition(this Type type)
        => type.IsGenericType ? type.GetGenericTypeDefinition() : type;

    public static IEnumerable<T> WithAttribute<T, TAttribute>(this IEnumerable<T> src)
        where T : MemberInfo
        where TAttribute : Attribute
        => src.Where(m => m.HasCustomAttribute<TAttribute>());

    public static string ToInfoString(this MethodInfo mi, bool includeNamespace = false)
    {
        StringBuilder sb = new();
        if (mi.ReflectedType != mi.DeclaringType && mi.ReflectedType is not null)
            sb.Append(mi.ReflectedType.FullName(includeNamespace)).Append(':');
        if (mi.DeclaringType is not null)
            sb.Append(mi.DeclaringType.FullName(includeNamespace));

        if (mi.IsGenericMethod)
        {
            sb.Append('<');
            foreach (var typeArg in mi.GetGenericArguments())
                sb.Append(typeArg.FullName(includeNamespace));
            sb.Append('>');
        }
        sb.Append('(');
        sb.AppendJoin(", ", mi.GetParameters().Select(p =>
            $"{p.GetType().FullName(includeNamespace)} {p.Name} {(p.HasDefaultValue ? $" = {p.DefaultValue}" : "")}"));
        sb.Append(')');

        return sb.ToString();
    }

    #region get property nullabillity

    // source: https://stackoverflow.com/questions/58453972/how-to-use-net-reflection-to-check-for-nullable-reference-type
    public static bool IsNullable(this PropertyInfo property) =>
        IsNullableHelper(property.PropertyType, property.DeclaringType, property.CustomAttributes);

    public static bool IsNullable(this FieldInfo field) =>
        IsNullableHelper(field.FieldType, field.DeclaringType, field.CustomAttributes);

    public static bool IsNullable(this ParameterInfo parameter) =>
        IsNullableHelper(parameter.ParameterType, parameter.Member, parameter.CustomAttributes);

    private static bool IsNullableHelper(Type memberType, MemberInfo? declaringType,
        IEnumerable<CustomAttributeData> customAttributes)
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
                .FirstOrDefault(x =>
                    x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            if (context is { ConstructorArguments.Count: 1 } &&
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