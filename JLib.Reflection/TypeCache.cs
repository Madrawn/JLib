using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;
using Microsoft.Extensions.Logging;

namespace JLib.Reflection;

/// <summary>
/// groups <see cref="Type"/>s by <see cref="TypeValueType"/>, validates them and initializes Navigation
/// <br/> Service interface for the <see cref="TypeCache"/>.
/// <br/>- <seealso cref="TypeValueType"/>
/// <br/>- <seealso cref="NavigatingTypeValueType"/>
/// <br/>- <seealso cref="IValidatedType"/>
/// <br/>- <seealso cref="IPostNavigationInitializedType"/>
/// <br/>- <seealso cref="TvtFactoryAttribute"/>
/// <br/>- <seealso cref="IgnoreInCache"/>
/// </summary>
public interface ITypeCache
{
    /// <summary>
    /// the <see cref="Type"/>s of all known <see cref="TypeValueType"/>s (not their instances)
    /// </summary>
    public IReadOnlyCollection<Type> KnownTypeValueTypes { get; }

    /// <summary>
    /// all types known to the typeCache without any filters or applied valueTypes
    /// </summary>
    public IReadOnlyCollection<Type> KnownTypes { get; }

    /// <returns>the single instance of <typeparamref name="TTvt"/> which satisfies the given <paramref name="filter"/></returns>
    /// <exception cref="TypeValueTypeMismatchException{TRequestedTypeValueType}"></exception>
    /// <exception cref="UnknownTypeException{TRequestedTypeValueType}"></exception>
    /// <exception cref="TypeNotFoundException{TRequestedTypeValueType}"></exception>
    /// <exception cref="NotUniqueTypeFilterException{TRequestedTypeValueType}"></exception>
    public TTvt Get<TTvt>(Func<TTvt, bool> filter) where TTvt : class, ITypeValueType
    {
        var items = All<TTvt>().Where(filter).ToReadOnlyCollection();
        return items.Count switch
        {
            1 => items.Single(),
            > 1 => throw new NotUniqueTypeFilterException<TTvt>(),
            0 => throw new TypeNotFoundException<TTvt>(),
            _ => throw new IndexOutOfRangeException("a negative index is impossible")
        };
    }

    /// <returns>The <typeparamref name="TTvt"/> instance of the given <paramref name="weakType"/></returns>
    /// <exception cref="TypeValueTypeMismatchException{TRequestedTypeValueType}"></exception>
    /// <exception cref="UnknownTypeException{TRequestedTypeValueType}"></exception>
    /// <exception cref="TypeNotFoundException{TRequestedTypeValueType}"></exception>
    public TTvt Get<TTvt>(Type weakType) where TTvt : class, ITypeValueType;

    /// <returns>The <typeparamref name="TTvt"/> instance of the given <typeparamref name="TType"/></returns>
    /// <exception cref="TypeValueTypeMismatchException{TRequestedTypeValueType}"></exception>
    /// <exception cref="UnknownTypeException{TRequestedTypeValueType}"></exception>
    /// <exception cref="TypeNotFoundException{TRequestedTypeValueType}"></exception>
    public TTvt Get<TTvt, TType>() where TTvt : class, ITypeValueType
        => Get<TTvt>(typeof(TType));

    /// <returns>the single instance of <typeparamref name="TTvt"/> which satisfies the given <paramref name="filter"/></returns>
    /// <exception cref="NotUniqueTypeFilterException{TRequestedTypeValueType}"></exception>
    public TTvt? TryGet<TTvt>(Func<TTvt, bool> filter) where TTvt : class, ITypeValueType
    {
        var res = All<TTvt>().Where(filter).ToReadOnlyCollection();
        return res.Count switch
        {
            1 => res.Single(),
            > 1 => throw new NotUniqueTypeFilterException<TTvt>(),
            0 => null,
            _ => throw new IndexOutOfRangeException("a negative index is impossible")
        };
    }

    /// <returns>The <typeparamref name="TTvt"/> instance of the given <paramref name="weakType"/></returns>
    public TTvt? TryGet<TTvt>(Type? weakType) where TTvt : class, ITypeValueType;

    /// <returns>The <typeparamref name="TTvt"/> instance of the given <typeparamref name="TType"/></returns>
    public TTvt? TryGet<TTvt, TType>() where TTvt : class, ITypeValueType
        => TryGet<TTvt>(typeof(TType).TryGetGenericTypeDefinition());
    /// <returns>All <see cref="TypeValueType"/>s assignable to <typeparamref name="TTvt"/></returns>
    public IEnumerable<TTvt> All<TTvt>() where TTvt : class, ITypeValueType;

    /// <summary>
    /// The <see cref="ITypePackage"/> which was used to create this <see cref="ITypeCache"/>
    /// </summary>
    public ITypePackage TypePackage { get; }
}

/// <summary>
/// provides an easy-to-use way to group types by kind, i.e. entities
/// <br/>searches the Application for <see cref="TypeValueType"/> instances with <see cref="TvtFactoryAttribute.ITypeValueTypeFilterAttribute"/> attributes
/// and populates them with the types provided via constructor.
/// <br/> all reflection is done in the constructor
/// <br/> should be used as singleton
/// </summary>
public class TypeCache : ITypeCache
{
    private record ValueTypeForTypeValueTypes : ValueType<Type>
    {
        public ValueTypeForTypeValueTypes(Type Value) : base(Value)
        {
            if (!Value.IsAssignableTo(typeof(TypeValueType)))
                throw new InvalidSetupException($"{Value.Name} does not derive from {nameof(TypeValueType)}");
            if (Value.IsAbstract)
                throw new InvalidSetupException($"{Value.Name} is abstract");
        }

        public bool Filter(Type otherType)
            => Value.GetCustomAttributes()
                .OfType<TvtFactoryAttribute>()
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
    private readonly ILogger _logger;

    /// <summary>
    /// <inheritdoc cref="ITypeCache.KnownTypeValueTypes"/>
    /// </summary>
    public IReadOnlyCollection<Type> KnownTypeValueTypes { get; }

    /// <summary>
    /// <inheritdoc cref="ITypeCache.KnownTypes"/>
    /// </summary>
    public IReadOnlyCollection<Type> KnownTypes { get; }

    /// <summary>
    /// <inheritdoc cref="ITypeCache.TypePackage"/>
    /// </summary>
    public ITypePackage TypePackage { get; }

    #region constructor
    /// <summary>
    /// creates an instance of <see cref="TypeCache"/> and initializes all <see cref="TypeValueType"/>s
    /// </summary>
    /// <param name="typePackage"></param>
    /// <param name="parentExceptionManager"></param>
    /// <param name="loggerFactory"></param>
    public TypeCache(ITypePackage typePackage, ExceptionBuilder parentExceptionManager, ILoggerFactory loggerFactory)
    {
        TypePackage = typePackage;
        _logger = loggerFactory.CreateLogger(typeof(ITypeCache)?.FullName ?? nameof(ITypeCache));
        using var _ = _logger.BeginScope(this);
        KnownTypes = typePackage.GetContent().ToArray();
        const string exceptionMessage = "Cache setup failed";
        var exceptions = parentExceptionManager.CreateChild(exceptionMessage);

        var availableTypeValueTypes = KnownTypes
            .Where(type => !type.HasCustomAttribute<IgnoreInCache>())
            .Where(type => type.IsAssignableTo<TypeValueType>() && !type.IsAbstract)
            .Select(tvt => new ValueTypeForTypeValueTypes(tvt))
            .ToArray();
        KnownTypeValueTypes = availableTypeValueTypes.Select(tvtt => tvtt.Value).ToArray();

        exceptions.CreateChild(
            "some Types have no filter attributes",
            availableTypeValueTypes.Where(tvtt => tvtt.Value
                .CustomAttributes.None(a =>
                    a.AttributeType.IsAssignableTo<TvtFactoryAttribute>())
            ).Select(tvtt => new InvalidTypeException(tvtt.GetType(), tvtt.Value,
                tvtt.Value.FullName(true)))
        );
        var discoveryExceptions = exceptions.CreateChild("type discovery failed");
        try
        {
            _typeValueTypes = KnownTypes
                .Where(type => !type.HasCustomAttribute<IgnoreInCache>() && !type.IsAssignableTo<TypeValueType>())
                .Select(type =>
                {
                    try
                    {
                        var validTvtGroups = availableTypeValueTypes
                            .Where(availableTvtt => availableTvtt.Filter(type))
                            .ToLookup(t =>
                                t.Value.GetCustomAttribute<TvtFactoryAttribute.PriorityAttribute>()?.Value
                                ?? TvtFactoryAttribute.PriorityAttribute.DefaultPriority);
                        var validTvts = validTvtGroups.MinBy(x => x.Key)?
                            .ToArray() ?? Array.Empty<ValueTypeForTypeValueTypes>();
                        switch (validTvts.Length)
                        {
                            case > 1:
                                discoveryExceptions.Add(new InvalidSetupException(
                                    $"multiple tvt candidates found for type {type.Name} : " +
                                    $@"[ {string.Join(", ", validTvts.Select(tvt =>
                                    {
                                        var priority = tvt.Value.GetCustomAttribute<TvtFactoryAttribute.PriorityAttribute>()?.Value
                                                       ?? TvtFactoryAttribute.PriorityAttribute.DefaultPriority;
                                        return $"{tvt.Value.Name}(priority {priority})";
                                    }).OrderBy(d => d))} ]"));
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

        foreach (var typeValueType in _typeValueTypes.OfType<IPostNavigationInitializedType>())
        {
            try
            {
                typeValueType.Initialize(this, exceptions.CreateChild("Initialization failed"));
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
                var tvtValidator = new TypeValidationContext(typeValueType.CastTo<TypeValueType>(),
                    typeValueType.GetType());
                typeValueType.Validate(this, tvtValidator);
                exceptions.AddChild(tvtValidator);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        WriteLog();
    }

    #endregion

    /// <summary>
    /// <inheritdoc cref="ITypeCache.Get{TTvt}(System.Type)"/>
    /// </summary>
    public T Get<T>(Type weakType)
        where T : class, ITypeValueType
    {
        var strongType = _typeMappings.GetValueOrDefault(weakType);
        if (strongType is null)
        {
            if (KnownTypes.Contains(weakType))
                throw new TypeNotFoundException<T>(weakType);
            throw new UnknownTypeException<T>(weakType);
        }
        return strongType as T ?? throw new TypeValueTypeMismatchException<T>(strongType.GetType(), weakType);
    }

    /// <summary>
    /// <inheritdoc cref="ITypeCache.TryGet{TTvt}(System.Type?)"/>
    /// </summary>
    public T? TryGet<T>(Type? weakType)
        where T : class, ITypeValueType
        => weakType is null
            ? null
            : _typeMappings.TryGetValue(weakType, out var tvt)
                ? tvt.As<T?>()
                : null;

    /// <summary>
    /// <inheritdoc cref="ITypeCache.All{TTvt}"/>
    /// </summary>
    public IEnumerable<T> All<T>()
        where T : class, ITypeValueType
        => _typeValueTypes.OfType<T>();

    /// <summary>
    /// writes the contents of the <see cref="TypeCache"/> to the <see cref="ILogger"/>
    /// </summary>
    public void WriteLog()
    {
        using var _ = _logger.BeginScope(this);
        _logger.LogInformation("Initialized TypeCache with a total of {typeCount} types", _typeValueTypes.Length);
        WriteDebug();

        var missing = KnownTypeValueTypes.Except(_typeValueTypes.Select(x => x.GetType()).Distinct()).ToArray();
        if (missing.Any())
            _logger.LogWarning("  No types found for: {TypeValueTypeName}", missing);
        return;

        void WriteDebug()
        {
            if (!_logger.IsEnabled(LogLevel.Debug))
                return;

            var typesByAssembly = _typeValueTypes
                .ToLookup(tvt => tvt.Value.Assembly.FullName)
                .OrderBy(g => g.Key)
                .ToArray();

            foreach (var typesInAssembly in typesByAssembly)
            {
                _logger.LogDebug("  Found {typeCount} types in Assemlby {assemblyName}", typesInAssembly.Count(),
                    typesInAssembly.Key);
                WriteTypes(typesInAssembly);
            }
            //Log.Verbose("  Total Types:");
            //WriteTypes(_typeValueTypes);
        }

        void WriteTypes(IEnumerable<TypeValueType> types)
        {
            var registeredTypes = types
                .ToLookup(tvt => tvt.GetType())
                .OrderBy(g => g.Key.Name)
                .ToArray();
            foreach (var group in registeredTypes)
            {
                _logger.LogDebug("    ValueTypeType     + {TypeValueTypeName}", group.Key);

                if (!_logger.IsEnabled(LogLevel.Trace))
                    continue;
                foreach (var tvt in group)
                    _logger.LogTrace("      DiscoveredType    - {TypeName}", tvt.Name);
            }
        }
    }
}


/// <summary>
/// indicates, that the <see cref="TypeCache"/> threw an exception
/// </summary>
public abstract class TypeCacheException : JLibException
{
    internal TypeCacheException(string message) : base(message)
    {
    }

    internal TypeCacheException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Indicates, that the <see cref="GivenType"/> could not be resolved as <see cref="RequestedTypeValueType"/>
/// </summary>
public abstract class TypeResolverException : TypeCacheException
{
    public Type RequestedTypeValueType { get; }
    public Type? GivenType { get; }

    internal TypeResolverException(Type requestedTypeValueType, Type? givenType, string message) : base(message)
    {
        RequestedTypeValueType = requestedTypeValueType;
        Data[nameof(RequestedTypeValueType)] = requestedTypeValueType;
        GivenType = givenType;
        Data[nameof(GivenType)] = givenType;
    }
}

/// <summary>
/// Indicates, that the <see cref="TypeNotFoundException.GivenType"/> is not registered in the <see cref="ITypeCache"/> under any <see cref="TypeValueType"/>
/// </summary>
public abstract class TypeNotFoundException : TypeResolverException
{
    internal TypeNotFoundException(Type requestedTypeValueType, Type? givenType) : base(requestedTypeValueType, givenType, $"The TypeCache does not contain an instance of  {givenType?.FullName(true)}")
    {
    }
}

/// <summary>
/// Indicates, that the <see cref="ITypeCache"/> does not contain any return any <see cref="TypeValueType"/>s which are assignable to <typeparamref name="TRequestedTypeValueType"/> and are either the <see cref="TypeResolverException.GivenType"/> or satisfy a given filter.
/// </summary>
public sealed class TypeNotFoundException<TRequestedTypeValueType> : TypeNotFoundException
    where TRequestedTypeValueType : ITypeValueType
{
    /// <summary>
    /// Indicates, that the filter did not return any <see cref="TypeValueType"/>s in the <see cref="ITypeCache"/> under any <typeparamref name="TRequestedTypeValueType"/>
    /// </summary>
    internal TypeNotFoundException() : base(typeof(TRequestedTypeValueType), null)
    {
    }
    /// <summary>
    /// Indicates, that the filter did not return any <see cref="TypeValueType"/>s in the <see cref="ITypeCache"/> under any <typeparamref name="TRequestedTypeValueType"/>
    /// </summary>
    internal TypeNotFoundException(Type givenType) : base(typeof(TRequestedTypeValueType), givenType)
    {
    }
}

/// <summary>
/// Indicates, that the filter expression used to retrieve a single <see cref="TypeResolverException.RequestedTypeValueType"/> from the <see cref="ITypeCache"/> is true for more than one <see cref="TypeValueType"/>
/// </summary>
public abstract class NotUniqueTypeFilterException : TypeResolverException
{
    internal NotUniqueTypeFilterException(Type requestedTypeValueType) : base(requestedTypeValueType, null, $"The TypeCache does contain multiple valueTypes assignable to {requestedTypeValueType.FullName()} which satisfy the given condition")
    {
    }
}
/// <summary>
/// Indicates, that the filter expression used to retrieve a single <see cref="TypeResolverException.RequestedTypeValueType"/> from the <see cref="ITypeCache"/> is true for more than one <see cref="TypeValueType"/>
/// </summary>
public sealed class NotUniqueTypeFilterException<TRequestedTypeValueType> : NotUniqueTypeFilterException
    where TRequestedTypeValueType : ITypeValueType
{
    internal NotUniqueTypeFilterException() : base(typeof(TRequestedTypeValueType))
    {
    }
}

/// <summary>
/// Indicates, that the <see cref="TypePackage"/> passed to the <see cref="ITypeCache"/> did not contain the <see cref="TypeResolverException.GivenType"/>
/// </summary>
public abstract class UnknownTypeException : TypeResolverException
{
    internal UnknownTypeException(Type requestedTypeValueType, Type givenType) : base(requestedTypeValueType, givenType, $"The TypePackage passed to the TypeCache did not contain {givenType.FullName(true)}")
    {
    }
}
/// <summary>
/// Indicates, that the <see cref="TypePackage"/> passed to the <see cref="ITypeCache"/> did not contain the <see cref="TypeResolverException.GivenType"/>
/// </summary>
public sealed class UnknownTypeException<TRequestedTypeValueType> : UnknownTypeException
    where TRequestedTypeValueType : ITypeValueType
{
    internal UnknownTypeException(Type givenType) : base(typeof(TRequestedTypeValueType), givenType)
    {
    }
}

/// <summary>
/// Indicates, that the <see cref="TypeResolverException.GivenType"/> was found in the <see cref="ITypeCache"/> but it was not associated with the expected <see cref="TypeResolverException.RequestedTypeValueType"/> but instead with <see cref="ActualTypeValueType"/>, which are not assignable
/// </summary>
public abstract class TypeValueTypeMismatchException : TypeResolverException
{
    /// <summary>
    /// The Actual <see cref="TypeValueType"/> under which the <see cref="TypeResolverException.GivenType"/> is registered in the <see cref="ITypeCache"/>
    /// </summary>
    public Type ActualTypeValueType { get; }

    internal TypeValueTypeMismatchException(Type requestedTypeValueType, Type actualTypeValueType, Type givenType) : base(requestedTypeValueType, givenType, $"{givenType.FullName(true)} was requested as {requestedTypeValueType.FullName(true)} which is not assignable to its actual {nameof(TypeValueType)} of {actualTypeValueType.FullName(true)}")
    {
        ActualTypeValueType = actualTypeValueType;
        Data[nameof(ActualTypeValueType)] = actualTypeValueType;
    }
}
/// <summary>
/// Indicates, that the <see cref="TypeResolverException.GivenType"/> was found in the <see cref="ITypeCache"/> but it was not associated with the expected <typeparamref cref="TRequestedTypeValueType"/> but instead with <see cref="TypeValueTypeMismatchException.ActualTypeValueType"/>, which are not assignable
/// </summary>
public sealed class TypeValueTypeMismatchException<TRequestedTypeValueType> : TypeValueTypeMismatchException
    where TRequestedTypeValueType : ITypeValueType
{
    internal TypeValueTypeMismatchException(Type actualTypeValueType, Type givenType) : base(typeof(TRequestedTypeValueType), actualTypeValueType, givenType)
    {
    }
}