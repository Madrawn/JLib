using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using AutoMapper.Internal;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;
using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.DataGeneration;

[IsDerivedFrom(typeof(DataPackage)), NotAbstract]
public record DataPackageType(Type Value) : TypeValueType(Value), IValidatedType
{
    public void Validate(ITypeCache cache, TypeValidationContext value)
    {
        value.ShouldBeSealed("a DataPackage has to be either Sealed or Abstract.");

        value.ValidateProperties(p => p.IsPublic(), p => p
            .HavePublicInit()
            .HavePublicGet());
    }
}

/// <summary>
/// may be used inside a <see cref="DataPackage"/> to skip the assignment of the id property.
/// This may be used to create public, non id properties
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SkipIdAssignmentAttribute : Attribute { }

public abstract class DataPackage
{
    /// <summary>
    /// contains the binding flags which will be used to discover id properties
    /// </summary>
    public const BindingFlags PropertyDiscoveryBindingFlags = BindingFlags.Instance | BindingFlags.Public;
    public string GetInfoText(string propertyName)
    {
        var property = GetType().GetProperty(propertyName, PropertyDiscoveryBindingFlags) ??
                       throw new InvalidSetupException(
                           $"property {propertyName} not found on {GetType().FullName()}");
        return new DataPackageValues.IdGroupName(property).Value + "." + property.Name;
    }

    protected DataPackage(IServiceProvider provider)
    {
        var packageManager = provider.GetRequiredService<IDataPackageManager>();
        switch (packageManager.InitState)
        {
            case DataPackageInitState.Uninitialized:
                throw new InvalidOperationException(
                    "invalid injection. inject directly after provider build using 'JLib.DataGeneration.DataPackageExtensions.IncludeDataPackages'.");
            case DataPackageInitState.Initialized:
                throw new InvalidOperationException(
                    "invalid injection: this type package has not been included when calling 'JLib.DataGeneration.DataPackageExtensions.IncludeDataPackages'.");
            case DataPackageInitState.Initializing:
                break;
            default:
                throw new IndexOutOfRangeException(nameof(packageManager.InitState));
        }

        var propertyInfos = GetType().GetProperties(PropertyDiscoveryBindingFlags)
            .Where(x =>
                x.HasCustomAttribute<SkipIdAssignmentAttribute>() is false
            ).ToReadOnlyCollection();

        propertyInfos.GroupBy(x => x.Name)
            .Where(x => x.Count() > 1)
            .Select(x => new AggregateException($"property {x.Key} is defined multiple times.", x.Select(y => new Exception(y.ToDebugInfo()))))
            .ThrowExceptionIfNotEmpty("duplicate properties found");

        foreach (var propertyInfo in propertyInfos)
            packageManager.SetIdPropertyValue(this, propertyInfo);
    }
}