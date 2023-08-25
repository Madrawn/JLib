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
    private record ValueTypeForTypeValueTypes : ValueType<Type>
    {
        public ValueTypeForTypeValueTypes(Type Value) : base(Value)
        {
            if (!Value.IsAssignableTo(typeof(TypeValueType)))
                throw new TvtNavigationFailedException($"{Value.Name} does not derive from {nameof(TypeValueType)}");
            if (Value.IsAbstract)
                throw new TvtNavigationFailedException($"{Value.Name} is abstract");
        }

        public bool Filter(Type otherType)
            => Value.GetCustomAttributes()
                .OfType<TvtFactoryAttributes.ITypeValueTypeFilterAttribute>()
                .All(filterAttr => filterAttr.Filter(otherType));
        public TypeValueType Create(Type type)
        {
            var ctor = Value.GetConstructor(new[] { typeof(Type) })
                ?? throw new InvalidTypeException(Value, Value, $"ctor not found for {Value.Name}");
            var instance = ctor.Invoke(new object[] { type })
                ?? throw new InvalidSetupException($"ctor could not be invoked for {Value.Name}");
            return instance as TypeValueType
                ?? throw new InvalidSetupException($"instance of {Value} is not a {nameof(TypeValueType)}");
        }
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
        var exceptions = new List<Exception>();

        var rawTypes = types
            .Where(type => type.IsAssignableTo<TypeValueType>() && !type.IsAbstract)
            .Select(tvt => new ValueTypeForTypeValueTypes(tvt))
            .ToArray();

        rawTypes.Where(tvtt => tvtt.Value
                .CustomAttributes.None(a => a.AttributeType.Implements<TvtFactoryAttributes.ITypeValueTypeFilterAttribute>())
        ).Select(tvtt => new InvalidTypeException(tvtt.GetType(), tvtt.Value, $"{tvtt.Value.Name} does not have any filter attribute added"))
            .ThrowIfNotEmpty("some TypeValueTypes have no filter attributes");


        _typeValueTypes = types
            .Select(type =>
            {
                var validTvts = rawTypes.Where(tvtt => tvtt.Filter(type)).ToArray();
                switch (validTvts.Length)
                {
                    case > 1:
                        exceptions.Add(new InvalidSetupException(
                            $"multiple tvt candidates found for type {type.Name} : " +
                            $"[ {string.Join(", ", validTvts.Select(tvt => tvt.Value.Name))} ]"));
                        return null;
                    case 0:
                        return null;
                    default:
                        return validTvts.Single().Create(type);
                }
            }).WhereNotNull()
            .ToArray();

        _typeMappings = _typeValueTypes.ToDictionary(tvt => tvt.Value);

        foreach (var typeValueType in _typeValueTypes.OfType<NavigatingTypeValueType>())
        {
            try
            {
                typeValueType.SetCache(this);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        foreach (var typeValueType in _typeValueTypes.OfType<NavigatingTypeValueType>())
        {
            try
            {
                typeValueType.MaterializeNavigation();
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        foreach (var typeValueType in _typeValueTypes.OfType<IPostInitValidatedType>())
        {
            try
            {
                typeValueType.PostInitValidation(this);

            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }
        exceptions.ThrowIfNotEmpty("typeCache could not be initialized or validated");
    }


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
