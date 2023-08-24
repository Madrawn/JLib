using System.Reflection;

using JLib.Exceptions;
using JLib.FactoryAttributes;
using JLib.Helper;

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

public class TypeCache : ITypeCache
{
    private record ValueTypeForValueTypes(Type Value) : TypeValueType(Value)
    {
        public bool Filter(Type otherType)
            => Value.GetCustomAttributes()
                .OfType<TvtFactoryAttributes.ITypeValueTypeFilterAttribute>()
                .All(filterAttr => filterAttr.Filter(otherType));
        public TypeValueType Create(Type value, ITypeCache typeCache)
            => Value.GetConstructor(new[] { typeof(Type), typeof(ITypeCache) })
                ?.Invoke(null, new object[] { value, typeCache })
                ?.As<TypeValueType>()
               ?? throw new InvalidSetupException($"not tvt ctor found for {Value.Name}");
    }

    private class PostInitTypeCache : ITypeCache
    {
        public T Get<T>(Type weakType) where T : TypeValueType => throw new NotImplementedException();

        public T? TryGet<T>(Type weakType) where T : TypeValueType => throw new NotImplementedException();
        public IEnumerable<T> All<T>() where T : TypeValueType => throw new NotImplementedException();
    }

    private readonly TypeValueType[] _typeValueTypes;
    private readonly IReadOnlyDictionary<Type, TypeValueType> _typeMappings;

    public TypeCache(params Assembly[] assemblies) : this(assemblies.AsEnumerable()) { }
    public TypeCache(IEnumerable<Assembly> assemblies) : this(assemblies.SelectMany(a => a.GetTypes())) { }
    public TypeCache(IEnumerable<Type> types) : this(types.ToArray()) { }
    public TypeCache(params Type[] types)
    {
        var rawTypes = GetTypeValueTypes(types).ToArray();

        var exceptions = new List<Exception>();

        _typeValueTypes = types.Select(type =>
        {
            var validTvts = rawTypes.Where(tvtt => tvtt.Filter(type)).ToArray();
            switch (validTvts.Length)
            {
                case > 1:
                    exceptions.Add(new InvalidSetupException($"multiple tvt candidates found for type {type.Name}:" +
                                                             string.Join(", ", validTvts.Select(tvt => tvt.Name))));
                    return null;
                case 0:
                    return null;
                default:
                    return validTvts.Single().Create(type, this);
            }
        }).WhereNotNull()
            .ToArray();

        _typeMappings = _typeValueTypes.ToDictionary(tvt => tvt.Value);

        foreach (var typeValueType in _typeValueTypes.OfType<NavigatingTypeValueType>())
            typeValueType.SetCache(this);
        foreach (var typeValueType in _typeValueTypes.OfType<NavigatingTypeValueType>())
            typeValueType.MaterializeNavigation();
        foreach (var typeValueType in _typeValueTypes.OfType<IPostInitValidatedType>())
            typeValueType.PostInitValidation(this);
    }


    private static IEnumerable<ValueTypeForValueTypes> GetTypeValueTypes(IEnumerable<Type> types)
        => types
            .Where(type => type.IsDerivedFromAny<ValueType<object>>() && !type.IsAbstract)
            .Select(tvt => new ValueTypeForValueTypes(tvt));

    public T Get<T>(Type weakType)
        where T : TypeValueType
        => _typeMappings[weakType].CastTo<T>();

    public T? TryGet<T>(Type weakType)
        where T : TypeValueType
        => _typeMappings.TryGetValue(weakType, out var tvt)
            ? tvt.As<T?>()
            : null;

    public IEnumerable<T> All<T>()
        where T : TypeValueType
        => _typeValueTypes.OfType<T>();
}
