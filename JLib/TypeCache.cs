using System.Reflection;

using JLib.FactoryAttributes;
using JLib.Helper;

using Microsoft.Extensions.DependencyInjection;

namespace JLib;

public interface ISubCache<T>
{

}

public interface ITypeCache
{
    public T Get<T>(Type weakType) where T : TypeValueType;
    public T? TryGet<T>(Type weakType) where T : TypeValueType;
    public IEnumerable<T> All<T>() where T : TypeValueType;
}

public class TypeCache
{
    private record ValueTypeForValueTypes(Type Value) : TypeValueType(Value)
    {
        public bool Filter(Type otherType)
            => Value.GetCustomAttributes()
                .OfType<TvtFactoryAttributes.ITypeValueTypeFilterAttribute>()
                .All(filterAttr => filterAttr.Filter(otherType));

    }

    private class PostInitTypeCache : ITypeCache
    {
        public T Get<T>(Type weakType) where T : TypeValueType => throw new NotImplementedException();

        public T? TryGet<T>(Type weakType) where T : TypeValueType => throw new NotImplementedException();
        public IEnumerable<T> All<T>() where T : TypeValueType => throw new NotImplementedException();
    }


    public TypeCache(params Assembly[] assemblies) : this(assemblies.AsEnumerable()) { }
    public TypeCache(IEnumerable<Assembly> assemblies) : this(assemblies.SelectMany(a => a.GetTypes())) { }
    public TypeCache(IEnumerable<Type> types) : this(types.ToArray()) { }
    private static Type[] ctorParam = { typeof(Type) };
    public TypeCache(params Type[] types)
    {
        var typeValueTypes = GetTypeValueTypes(types).ToArray();
        var caches = typeValueTypes.ToDictionary(tvt => tvt,
                tvt => type => tvt.Value
                    .GetConstructor(ctorParam)
                    ?.Invoke(type ?? throw new($"Invalid Ctor on {tvt.Name}"))
                );
        var services = new ServiceCollection();
        foreach (var type in types)
        {
            var typeValueType = typeValueTypes.Single(tvt => tvt.Filter(type));
            caches[typeValueType].Add(type);
        }

    }


    private static IEnumerable<ValueTypeForValueTypes> GetTypeValueTypes(IEnumerable<Type> types)
        => types
            .Where(type => type.IsDerivedFromAny<ValueType<object>>() && !type.IsAbstract)
            .Select(tvt => new ValueTypeForValueTypes(tvt));
}
