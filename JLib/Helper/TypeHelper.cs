using System.Net.Http.Headers;

namespace JLib.Helper;

public static class TypeHelper
{
    public static IEnumerable<Type> GetDeclaringTypeTree(this Type type)
    {
        var cur = type;
        List<Type> res = new();
        while (cur.DeclaringType is not null)
        {
            res.Add(cur);
            cur = cur.DeclaringType;
        }

        return res;
    }

    /// <summary>
    /// a save variant of <see cref="TryGetGenericTypeDefinition"/>. If the type is not generic, it will simply return the given type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Type TryGetGenericTypeDefinition(Type type)
    {
        return !type.IsGenericType
            ? type
            : type.GetGenericTypeDefinition();
    }

    /// <summary>
    /// checks whether the given type is derived from the other type, ignoring all type parameters
    /// </summary>
    public static bool IsDerivedFromAny<T>(this Type type)
        => type.GetAnyBaseType<T>() is not null;
    /// <summary>
    /// <inheritdoc cref="IsDerivedFromAny{T}"/>
    /// </summary>
    public static bool IsDerivedFromAny(this Type type, Type baseType)
        => type.GetAnyBaseType(baseType) is not null;

    public static Type? GetAnyBaseType<T>(this Type type)
        => type.GetAnyBaseType(typeof(T));
    public static Type? GetAnyBaseType(this Type type, Type baseType)
    {
        var current = type;
        var compare = baseType.TryGetGenericTypeDefinition();
        while (current is not null && current.TryGetGenericTypeDefinition() != compare)
            current = current.BaseType;
        return current == compare
            ? null
            : current;
    }

    /// <summary>
    ///     checks if the type is an static class (i.e. abstract and sealed)
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsStatic(this Type type)
        => type is { IsClass: true, IsAbstract: true, IsSealed: true };

    public static bool IsAssignableTo<T>(this Type type)
        => type.IsAssignableTo(typeof(T));

    /// <summary>
    /// if T is generic, the type-parameters will be ignored. Use the <see cref="Ignored"/> type in this case when possible<br/>
    /// if you don't want it to be ignored, use <see cref="Implements{T}"/> instead
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool ImplementsAny<T>(this Type type)
        => type.GetInterface(typeof(T).Name) is not null || type.IsInterface &&
            (type.TryGetGenericTypeDefinition()) == (typeof(T).TryGetGenericTypeDefinition());

    public static bool ImplementsAny(this Type type, Type @interface)
        => type.GetInterface(@interface.Name) is not null;

    /// <summary>
    /// if T is generic, the type-parameters will NOT be ignored.<br/>
    /// if you want it to be ignored, use <see cref="ImplementsAny{T}"/> instead
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool Implements<T>(this Type type)
        => type.Implements(typeof(T));

    public static bool Implements(this Type type, Type @interface)
        => type.GetInterface(@interface.Name) == @interface;

    public static IEnumerable<Type> WhichImplement<T>(this IEnumerable<Type> collection)
        => collection.Where(t => t.ImplementsAny<T>());

    public static IEnumerable<Type> WhichAreAssignableTo<T>(this IEnumerable<Type> collection)
        => collection.WhichAreAssignableTo(typeof(T));

    public static IEnumerable<Type> WhichAreAssignableTo(this IEnumerable<Type> collection, Type type)
        => collection.Where(t => t.IsAssignableTo(type));

    public static IEnumerable<Type> WhichAreInstantiable(this IEnumerable<Type> collection)
        => collection.Where(t => !t.IsAbstract && !t.IsInterface);

    public static Type? GetInterface<TInterface>(this Type type)
    {
        var tInt = typeof(TInterface);
        return type.GetInterface(tInt.FullName ?? tInt.Name);
    }

    public static Type? GetAnyInterface<TInterface>(this Type type)
    {
        var tInt = typeof(TInterface);
        if (tInt.IsGenericType)
            tInt = tInt.GetGenericTypeDefinition();
        return type.GetInterfaces().FirstOrDefault(i => (i.IsGenericType ? i.GetGenericTypeDefinition() : i) == tInt);
    }

    public static bool HasSameGenericTypeDefinition<T>(this Type type)
        => HasSameGenericTypeDefinition(type, typeof(T));

    public static bool HasSameGenericTypeDefinition(this Type type, Type other)
    {
        var t1 = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        var t2 = other.IsGenericType ? other.GetGenericTypeDefinition() : other;
        return t1 == t2;
    }

    /// <summary>
    /// applies the given type parameters if they are applicable for the type
    /// <br/>if the type is not generic, the type is returned
    /// <br/>if the type arguments do not match but the type is generic, an exception will be thrown
    /// <br/>if the type is generic but the arguments are already filled, the arguments will be replaced
    /// </summary>
    public static Type TryMakeGenericType(this Type type, params Type[] genericParams)
    {
        type = type.TryGetGenericTypeDefinition();
        return type.IsGenericTypeDefinition
            ? type.GenericTypeArguments.SequenceEqual(genericParams)
                ? type.MakeGenericType(genericParams)
                : throw new InvalidOperationException("the type arguments do not match")
            : type;
    }

    public static Type MakeGenericType(this Type type, params ITypeValueType[] genericParams)
    {
        var typeArguments = genericParams.Select(x => x.Value).ToArray();
        try
        {
            return type.MakeGenericType(typeArguments);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"adding <{string.Join(", ", typeArguments.Select(x => x.FullClassName(true)))}> as typeArguments to type {type.FullClassName(true)} failed: {Environment.NewLine + e.Message}", e);
        }
    }

    /// <summary>
    /// the name of the class and its declaring type, excluding the namespace
    /// </summary>
    public static string FullClassName(this Type type, bool includeNamespace = false)
    {
        var name = (type.FullName ?? type.Name);
        string res = name
            .Split("[")
            .First(); ;
        if (!includeNamespace)
            res = res.Split(".").Last();
        res = res
            .Replace("+", ".")
            .Split("`").First();

        if (type.IsGenericType)
            res += $"<{string.Join(", ", type.GenericTypeArguments.Select(a => a.FullClassName(includeNamespace)))}>";

        return res;
    }

    public static TTvt CastValueType<TTvt>(this Type type, ITypeCache cache)
        where TTvt : TypeValueType
        => cache.Get<TTvt>(type);

    public static TTvt? AsValueType<TTvt>(this Type type, ITypeCache cache)
        where TTvt : TypeValueType
        => cache.TryGet<TTvt>(type);
}