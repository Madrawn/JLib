using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;
using static JLib.DataGeneration.DataPackageException.InitializationException.InvalidAccessException;
using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.DataGeneration;

/// <summary>
/// <see cref="TypeValueType"/> for <see cref="DataPackage"/>s
/// </summary>
[IsDerivedFrom(typeof(DataPackage)), NotAbstract]
public record DataPackageType : TypeValueType, IValidatedType
{
    internal IReadOnlyCollection<PropertyInfo> IdProperties { get; }

    internal const BindingFlags PropertyDiscoveryBindingFlags = BindingFlags.Instance | BindingFlags.Public;
    /// <summary>
    /// <inheritdoc cref="DataPackageType"/>
    /// </summary>
    public DataPackageType(Type value) : base(value)
    {
        IdProperties = GetType().GetProperties(PropertyDiscoveryBindingFlags)
            .Where(x =>
                x.HasCustomAttribute<SkipIdAssignmentAttribute>() is false
            ).ToReadOnlyCollection();
    }
    void IValidatedType.Validate(ITypeCache cache, TypeValidationContext value)
    {
        value.ShouldBeSealed("a DataPackage has to be either Sealed or Abstract.");

        value.ValidateProperties(p => IdProperties.Contains(p), p => p
            .HavePublicInit()
            .HavePublicGet());

        value.AddSubValidators(
            new ExceptionBuilder("duplicate properties found",
                IdProperties.GroupBy(x => x.Name)
                    .Where(x => x.Count() > 1)
                    .Select(x =>
                        new AggregateException($"property {x.Key} is defined multiple times.",
                            x.Select(y =>
                                new Exception(y.ToDebugInfo())
                            )
                        )
                    )
                    .ToArray<Exception>()
                ));

    }
}

/// <summary>
/// may be used inside a <see cref="DataPackage"/> to skip the assignment of the id property.
/// This may be used to create public, non id properties
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SkipIdAssignmentAttribute : Attribute { }

/// <summary>
/// Generates persistent, unique IDs and resolves dependencies using Dependency Injection.
/// </summary>
public abstract class DataPackage
{
    private readonly DataPackageManager _packageManager;

    internal DataPackageValues.IdIdentifier IdentifierOfIdProperty(PropertyInfo prop)
    => new(prop, _packageManager.Configuration);
    /// <summary>
    /// <inheritdoc cref="DataPackage"/>
    /// </summary>
    /// <param name="provider"></param>
    /// <exception cref="PreInitializationInstantiationException"></exception>
    /// <exception cref="PostInitializationAccessException"></exception>
    /// <exception cref="IndexOutOfRangeException"></exception>
    protected DataPackage(IServiceProvider provider)
    {
        _packageManager = provider.GetRequiredService<DataPackageManager>();
        switch (_packageManager.InitState)
        {
            case DataPackageInitState.Uninitialized:
                throw new PreInitializationInstantiationException(this);
            case DataPackageInitState.Initialized:
                throw new PostInitializationAccessException(this);
            case DataPackageInitState.Initializing:
                break;
            default:
                throw new InvalidInitStateException(this, _packageManager.InitState);
        }

        var typeCache = provider.GetRequiredService<ITypeCache>();
        var tvt = typeCache.Get<DataPackageType>(GetType());

        foreach (var propertyInfo in tvt.IdProperties)
            _packageManager.SetIdPropertyValue(this, propertyInfo);
    }
}