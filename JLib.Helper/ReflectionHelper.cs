using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace JLib.Helper;

/// <summary>
/// The access modifier of a <see cref="MemberInfo"/>
/// </summary>
public enum AccessModifier
{
    /// <summary>
    /// <see langword="private"/>
    /// </summary>
    Private,
    /// <summary>
    /// <see langword="protected"/>
    /// </summary>
    Protected,
    /// <summary>
    /// <see langword="internal"/>
    /// </summary>
    Internal,
    /// <summary>
    /// <see langword="protected internal"/>
    /// </summary>
    ProtectedInternal,
    /// <summary>
    /// <see langword="private protected"/>
    /// </summary>
    PrivateProtected,
    /// <summary>
    /// <see langword="public"/>
    /// </summary>
    Public
}
/// <summary>
/// Extension methods for reflection
/// </summary>
public static class ReflectionHelper
{
    /// <returns>the <see cref="AccessModifier"/> of the given <paramref name="methodInfo"/></returns>
    /// <exception cref="ArgumentException"></exception>
    public static AccessModifier GetAccessModifier(this MethodInfo methodInfo)
    {
        if (methodInfo.IsPrivate)
            return AccessModifier.Private;
        if (methodInfo.IsFamily)
            return AccessModifier.Protected;
        if (methodInfo.IsFamilyOrAssembly)
            return AccessModifier.ProtectedInternal;
        if (methodInfo.IsFamilyAndAssembly)
            return AccessModifier.PrivateProtected;
        if (methodInfo.IsAssembly)
            return AccessModifier.Internal;
        if (methodInfo.IsPublic)
            return AccessModifier.Public;
        throw new ArgumentException("Did not find access modifier", nameof(methodInfo));
    }
    /// <returns>the <see cref="AccessModifier"/> of the given <paramref name="fieldInfo"/></returns>
    /// <exception cref="ArgumentException"></exception>
    public static AccessModifier GetAccessModifier(this FieldInfo fieldInfo)
    {
        if (fieldInfo.IsPrivate)
            return AccessModifier.Private;
        if (fieldInfo.IsFamily)
            return AccessModifier.Protected;
        if (fieldInfo.IsFamilyOrAssembly)
            return AccessModifier.ProtectedInternal;
        if (fieldInfo.IsFamilyAndAssembly)
            return AccessModifier.PrivateProtected;
        if (fieldInfo.IsAssembly)
            return AccessModifier.Internal;
        if (fieldInfo.IsPublic)
            return AccessModifier.Public;
        throw new ArgumentException("Did not find access modifier", nameof(fieldInfo));
    }


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
    public static bool HasCustomAttribute(this MemberInfo type, Type attributeType, bool inherit = true)
        => type.GetCustomAttribute(attributeType, inherit) is not null;

    /// <summary>
    /// converts the member into a code-like representation.<br/>
    /// <b>unstable</b>
    /// <b>shall not be used on code but for debugging only</b>
    /// </summary>
    /// <param name="member"></param>
    /// <param name="includeNamespace"></param>
    /// <returns></returns>
    public static string ToDebugInfo(this MemberInfo member, bool includeNamespace = false)
    {
        var sb = new StringBuilder();
        if (member.DeclaringType != member.ReflectedType)
        {
            sb.Append('[').Append(member.ReflectedType?.FullName(includeNamespace)).Append(']');
        }

        sb.Append(member.DeclaringType?.FullName() ?? "");

        sb.Append('.');
        switch (member)
        {
            case MethodInfo methodInfo:
                sb.Insert(0, ' ').Insert(0, methodInfo.GetAccessModifier().ToString().ToLower());
                sb.Append(methodInfo.Name);
                if (methodInfo.IsGenericMethod)
                {
                    sb.Append('<');
                    sb.AppendJoin(", ", methodInfo.GetGenericArguments().Select(arg => $"{arg.FullName(includeNamespace)}"));
                    sb.Append('>');
                }
                if (methodInfo.IsGenericMethodDefinition)
                {
                    sb.Append('<');
                    sb.AppendJoin(", ", methodInfo.GetGenericArguments().Select(arg => $"{arg.Name}"));
                    sb.Append('>');
                }
                sb.Append('(');
                sb.AppendJoin(", ", methodInfo.GetParameters().Select(p => p.ParameterType.FullName(includeNamespace)));
                sb.Append(')');
                if (methodInfo.IsGenericMethodDefinition)
                {
                    foreach (var argument in methodInfo.GetGenericArguments())
                    {
                        var constraints = argument.GetGenericParameterConstraints();
                        if (constraints.None())
                            continue;
                        sb.Append("where ");
                        sb.Append(argument.Name).Append(" : ")
                            .AppendJoin(", ", constraints.Select(c => c.FullName(includeNamespace)));
                    }
                }
                break;
            case PropertyInfo propertyInfo:
                sb.Append(propertyInfo.Name)
                    .Append(" { ");
                if (propertyInfo.CanRead)
                {
                    sb.Append(propertyInfo.GetMethod?.GetAccessModifier().ToString().ToLower());
                    sb.Append(" get; ");
                }
                if (propertyInfo.CanWrite)
                {
                    sb.Append(propertyInfo.SetMethod?.GetAccessModifier().ToString().ToLower());
                    sb.Append(propertyInfo.IsInit() ? "init; " : "set; ");
                }
                sb.Append('}');
                break;
            case FieldInfo fieldInfo:
                sb.Append(fieldInfo.Name);
                if (fieldInfo.IsLiteral)
                    sb.Insert(0, "const ");
                if (fieldInfo.IsInitOnly)
                    sb.Insert(0, "readonly ");
                if (fieldInfo.IsStatic)
                    sb.Insert(0, "static ");
                sb.Insert(0, fieldInfo.GetAccessModifier()).Append(' ');
                break;
            case ConstructorInfo constructorInfo:
                sb.Append(constructorInfo.Name);
                sb.Append('(');
                sb.AppendJoin(", ", constructorInfo.GetParameters().Select(p => p.ParameterType.FullName(includeNamespace)));
                sb.Append(')');
                break;
            case EventInfo eventInfo:
                sb.Append(eventInfo.Name);
                break;
            default:
                sb.Append(member.Name);
                break;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Checks, whether the given <paramref name="type"/> is decorated with the given <typeparamref name="T"/>
    /// </summary>
    public static bool HasCustomAttribute<T>(this MemberInfo type, bool inherit = true)
        where T : Attribute =>
        type.HasCustomAttribute(typeof(T), inherit);

    /// <summary>
    /// <inheritdoc cref="Attribute.GetCustomAttributes(MemberInfo, Type, bool)"/>
    /// </summary>
    /// <typeparam name="T">the AttributeType to search for</typeparam>
    public static T[] GetCustomAttributes<T>(this MemberInfo type, bool inherit = true)
        where T : Attribute
        => Attribute.GetCustomAttributes(type, typeof(T), inherit).Cast<T>().ToArray();

    /// <summary>
    /// <inheritdoc cref="Attribute.GetCustomAttributes(MemberInfo, Type, bool)"/>
    /// </summary>
    public static Attribute[] GetCustomAttributes(this MemberInfo type, Type attributeType, bool inherit = true)
        => Attribute.GetCustomAttributes(type, attributeType, inherit).ToArray();

    /// <summary>
    /// <inheritdoc cref="Type.GetGenericTypeDefinition"/>
    /// </summary>
    public static Type TryGetGenericTypeDefinition(this Type type)
        => type.IsGenericType ? type.GetGenericTypeDefinition() : type;
    /// <summary>
    /// Executes <see cref="MethodInfo.GetGenericArguments"/> if the method is <see cref="MethodBase.IsGenericMethod"/> or <see cref="MethodBase.IsGenericMethodDefinition"/>, otherwise returns <see cref="Array.Empty{T}"/><br/>
    /// <inheritdoc cref="MethodInfo.GetGenericArguments"/>
    /// </summary>
    public static Type[] TryGetGenericArguments(this MethodInfo method)
        => (method.IsGenericMethodDefinition || method.IsGenericMethod) ? method.GetGenericArguments() : Array.Empty<Type>();

    /// <summary>
    /// filters the given <paramref name="src"/> for <see cref="Type"/>s decorated with the given <typeparamref name="TAttribute"/>
    /// </summary>
    /// <typeparam name="T">The type of the collection</typeparam>
    /// <typeparam name="TAttribute">The type of the attribute</typeparam>
    /// <param name="src">The source enumerable</param>
    /// <returns>The filtered enumerable</returns>
    public static IEnumerable<T> WithAttribute<T, TAttribute>(this IEnumerable<T> src)
        where T : MemberInfo
        where TAttribute : Attribute
        => src.Where(m => m.HasCustomAttribute<TAttribute>());

    /// <summary>
    /// formats the given <paramref name="methodInfo"/> signature to a string representing its prototype
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <param name="includeNamespace">whether to include the namespaces of all types</param>
    /// <returns>a string containing the <paramref name="methodInfo"/>'s prototype</returns>
    public static string ToInfoString(this MethodInfo methodInfo, bool includeNamespace = false)
    {
        StringBuilder sb = new();
        if (methodInfo.ReflectedType != methodInfo.DeclaringType && methodInfo.ReflectedType is not null)
            sb.Append(methodInfo.ReflectedType.FullName(includeNamespace)).Append(':');
        if (methodInfo.DeclaringType is not null)
            sb.Append(methodInfo.DeclaringType.FullName(includeNamespace));

        if (methodInfo.IsGenericMethod)
        {
            sb.Append('<');
            foreach (var typeArg in methodInfo.GetGenericArguments())
                sb.Append(typeArg.FullName(includeNamespace));
            sb.Append('>');
        }
        sb.Append('(');
        sb.AppendJoin(", ", methodInfo.GetParameters().Select(p =>
            $"{p.GetType().FullName(includeNamespace)} {p.Name} {(p.HasDefaultValue ? $" = {p.DefaultValue}" : "")}"));
        sb.Append(')');

        return sb.ToString();
    }

    #region get property nullabillity

    // source: https://stackoverflow.com/questions/58453972/how-to-use-net-reflection-to-check-for-nullable-reference-type

    /// <returns>whether the <paramref name="property"/> is nullable</returns>
    public static bool IsNullable(this PropertyInfo property) =>
        IsNullableHelper(property.PropertyType, property.DeclaringType, property.CustomAttributes);

    /// <returns>whether the <paramref name="field"/> is nullable</returns>
    public static bool IsNullable(this FieldInfo field) =>
        IsNullableHelper(field.FieldType, field.DeclaringType, field.CustomAttributes);

    /// <returns>whether the <paramref name="parameter"/> is nullable</returns>
    public static bool IsNullable(this ParameterInfo parameter) =>
        IsNullableHelper(parameter.ParameterType, parameter.Member, parameter.CustomAttributes);

    /// <returns>whether the <paramref name="genericTypeParameter"/> is nullable</returns>
    public static bool IsNullable(this Type genericTypeParameter, MemberInfo declaringType) =>
        IsNullableHelper(genericTypeParameter, declaringType, genericTypeParameter.CustomAttributes);

    private static bool IsNullableHelper(Type memberType, MemberInfo? declaringType,
        IEnumerable<CustomAttributeData> customAttributes)
    {
        if (memberType.IsValueType)
            return Nullable.GetUnderlyingType(memberType) != null;

        var nullable = customAttributes
            .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        if (nullable is { ConstructorArguments.Count: 1 })
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