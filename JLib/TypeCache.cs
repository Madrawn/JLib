using System.Reflection;

using JLib.FactoryAttributes;
using JLib.Helper;

using Microsoft.Extensions.DependencyInjection;

namespace JLib;

public interface ITypeCache
{
    T Get<T>(Type weakType) where T : class;
    T? TryGet<T>(Type weakType) where T : class;
}

public class TypeCache
{
    private record TypeValueTypeVt(Type Value) : TypeValueType(Value)
    {
        public bool Filter(Type otherType)
            => Value.GetCustomAttributes()
                .OfType<TvtFactoryAttributes.ITypeValueTypeFilterAttribute>()
                .All(filterAttr => filterAttr.Filter(otherType));

    }

    public TypeCache(params Assembly[] assemblies) : this(assemblies.AsEnumerable()) { }
    public TypeCache(IEnumerable<Assembly> assemblies) : this(assemblies.SelectMany(a => a.GetTypes())) { }
    public TypeCache(IEnumerable<Type> types) : this(types.ToArray()) { }
    public TypeCache(params Type[] types)
    {
        var typeValueTypes = GetTypeValueTypes(types).ToArray();
        var caches = typeValueTypes.ToDictionary(tvt => tvt,
                tvt => typeof(SubCache<>)
                    .MakeGenericType(tvt)
                    .GetConstructor(Array.Empty<Type>())
                    !.Invoke(Array.Empty<object>())
                );
        var services = new ServiceCollection();
        foreach (var type in types)
        {
            var typeValueType = typeValueTypes.Single(tvt => tvt.Filter(type));
            typeValueType.Value.const

        }

    }


    private static IEnumerable<TypeValueTypeVt> GetTypeValueTypes(IEnumerable<Type> types)
        => types
            .Where(type => type.IsDerivedFromAny<ValueType<object>>() && !type.IsAbstract)
            .Select(tvt => new TypeValueTypeVt(tvt));
}
