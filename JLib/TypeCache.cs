using System.Reflection;

using JLib.Exceptions;
using JLib.FactoryAttributes;
using JLib.Helper;
using Serilog;
using Serilog.Events;

namespace JLib;

public interface ISubCache<T>
{

}

public interface ITypeCache
{
    public IEnumerable<Type> KnownTypeValueTypes { get; }
    public TTvt Get<TTvt>(Type weakType) where TTvt : TypeValueType;
    public TTvt Get<TTvt, TType>() where TTvt : TypeValueType
        => Get<TTvt>(typeof(TType));
    public TTvt? TryGet<TTvt>(Type weakType) where TTvt : TypeValueType;
    public TTvt? TryGet<TTvt, TType>() where TTvt : TypeValueType
        => TryGet<TTvt>(typeof(TType).TryGetGenericTypeDefinition() ?? typeof(TType));
    public IEnumerable<TTvt> All<TTvt>() where TTvt : TypeValueType;
    public IEnumerable<TTvt> All<TTvt>(Func<TTvt, bool> filter) where TTvt : TypeValueType
        => All<TTvt>().Where(filter);
    public TTvt? SingleOrDefault<TTvt>(Func<TTvt, bool> selector) where TTvt : TypeValueType
    {
        var res = All<TTvt>().Where(selector).ToArray();
        if (res.Length > 1)
            throw new InvalidSetupException($"multiple selectors matched to be cast to {typeof(TTvt).Name}: " + string.Join(", ", res.Select(r => r.Name)));
        return res.FirstOrDefault();
    }

    public TTvt Single<TTvt>(Func<TTvt, bool> selector) where TTvt : TypeValueType
        => SingleOrDefault(selector) ?? throw new InvalidSetupException("no selector matched");

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

    private readonly TypeValueType[] _typeValueTypes;
    private readonly IReadOnlyDictionary<Type, TypeValueType> _typeMappings;
    public IEnumerable<Type> KnownTypeValueTypes { get; }

    public TypeCache(params Assembly[] assemblies) : this(assemblies.AsEnumerable()) { }
    public TypeCache(IEnumerable<Assembly> assemblies) : this(assemblies.SelectMany(a => a.GetTypes())) { }
    public TypeCache(IEnumerable<Type> types) : this(types.ToArray()) { }

    public TypeCache(params Type[] types)
    {
        IExceptionManager exceptions = new ExceptionManager("Cache setup failed");

        var availableTypeValueTypes = types
            .Where(type => !type.HasCustomAttribute<IgnoreInCache>())
            .Where(type => type.IsAssignableTo<TypeValueType>() && !type.IsAbstract)
            .Select(tvt => new ValueTypeForTypeValueTypes(tvt))
            .ToArray();
        KnownTypeValueTypes = availableTypeValueTypes.Select(tvtt => tvtt.Value).ToArray();

        exceptions.CreateChild(
            "some Types have no filter attributes",
                availableTypeValueTypes.Where(tvtt => tvtt.Value
                    .CustomAttributes.None(a => a.AttributeType.Implements<TvtFactoryAttributes.ITypeValueTypeFilterAttribute>())
                ).Select(tvtt => new InvalidTypeException(tvtt.GetType(), tvtt.Value, $"{tvtt.Value.Name} does not have any filter attribute added"))
            );
        var discoveryExceptions = exceptions.CreateChild("type discovery failed");
        try
        {
            _typeValueTypes = types
                .Where(type => !type.HasCustomAttribute<IgnoreInCache>())
                .Select(type =>
                {
                    try
                    {
                        var validTvts = availableTypeValueTypes
                            .Where(availableTvtt => availableTvtt.Filter(type))
                            .GroupBy(t =>
                                t.Value.GetCustomAttribute<TvtFactoryAttributes.Priority>()?.Value
                                ?? TvtFactoryAttributes.Priority.DefaultPriority)
                            .MinBy(x => x.Key)?
                            .ToArray() ?? Array.Empty<ValueTypeForTypeValueTypes>();
                        switch (validTvts.Length)
                        {
                            case > 1:
                                discoveryExceptions.Add(new InvalidSetupException(
                                    $"multiple tvt candidates found for type {type.Name} : " +
                                    $"[ {string.Join(", ", validTvts.Select(tvt => $"{tvt.Value.Name}(priority {tvt.Value.GetCustomAttribute<TvtFactoryAttributes.Priority>()?.Value ?? TvtFactoryAttributes.Priority.DefaultPriority})"))} ]"));
                                return null;
                            case 0:
                                return null;
                            default:
                                return validTvts.Single().Create(type);
                        }
                    }
                    catch (Exception e)
                    {
                        discoveryExceptions.Add(e);
                        return null;
                    }
                }).WhereNotNull()
                .ToArray();

            _typeMappings = _typeValueTypes.ToDictionary(tvt => tvt.Value);
        }
        catch (Exception ex)
        {
            discoveryExceptions.Add(ex);
            if (_typeValueTypes is null || _typeMappings is null)
                throw exceptions.GetException()!;
        }



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
            catch (TargetInvocationException e) when (e.InnerException is not null)
            {
                exceptions.Add(e.InnerException);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }
        foreach (var typeValueType in _typeValueTypes.OfType<IValidatedType>())
        {
            try
            {
                var tvtValidator = new TvtValidator((TypeValueType)typeValueType);
                typeValueType.Validate(this, tvtValidator);
                exceptions.AddChild(tvtValidator);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }
        exceptions.ThrowIfNotEmpty();

        WriteLog();
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

    public void WriteLog()
    {
        Log.ForContext<ITypeCache>().ForContext<ITypeCache>().Information("Initialized TypeCache with a total of {typeCount} types", _typeValueTypes.Length);
        WriteDebug();

        var missing = KnownTypeValueTypes.Except(_typeValueTypes.Select(x => x.GetType()).Distinct()).ToArray();
        if (missing.Any())
            Log.ForContext<ITypeCache>().Warning("  No types found for: {TypeValueTypeName}", missing);
        return;

        void WriteDebug()
        {
            if (!Log.IsEnabled(LogEventLevel.Debug))
                return;

            var typesByAssembly = _typeValueTypes
                .GroupBy(tvt => tvt.Value.Assembly.FullName)
                .OrderBy(g => g.Key)
                .ToArray();

            foreach (var typesInAssembly in typesByAssembly)
            {
                Log.ForContext<ITypeCache>().Debug("  Found {typeCount} types in Assemlby {assemblyName}", typesInAssembly.Count(), typesInAssembly.Key);
                WriteTypes(typesInAssembly);
            }
            //Log.Verbose("  Total Types:");
            //WriteTypes(_typeValueTypes);
        }

        void WriteTypes(IEnumerable<TypeValueType> types)
        {
            var registeredTypes = types
                .GroupBy(tvt => tvt.GetType())
                .OrderBy(g => g.Key.Name)
                .ToArray();
            foreach (var group in registeredTypes)
            {
                Log.ForContext<ITypeCache>().Debug("    ValueTypeType     + {TypeValueTypeName}", group.Key);

                if (!Log.IsEnabled(LogEventLevel.Verbose))
                    continue;
                foreach (var tvt in group)
                    Log.ForContext<ITypeCache>().Verbose("      DiscoveredType    - {TypeName}", tvt.Name);
            }
        }
    }
}
