using AutoMapper.Internal;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.DataGeneration;

[IsDerivedFrom(typeof(DataPackage)), NotAbstract]
public record DataPackageType(Type Value) : TypeValueType(Value), IValidatedType
{
    public void Validate(ITypeCache cache, TypeValidator value)
    {
        value.ShouldBeSealed("a DataPackage has to be either Sealed or Abstract.");

        value.ValidateProperties(p => p.IsPublic(), p => p
            .ShouldHavePublicInit()
            .ShouldHavePublicGet());
    }
}

public abstract class DataPackage
{
    public string GetInfoText(string propertyName)
    {
        var property = GetType().GetProperty(propertyName) ??
                       throw new InvalidSetupException(
                           $"property {propertyName} not found on {GetType().FullClassName()}");
        return new DataPackageValues.IdGroupName(property).Value + "." + property.Name;
    }

    protected DataPackage(IDataPackageManager packageManager)
    {
        switch (packageManager.InitState)
        {
            case DataPackageInitState.Uninitialized:
                throw new InvalidOperationException(
                    "invalid injection. inject directly after provider build using 'JLib.DataGeneration.DataPackageExtensions.IncludeDataPackages'.");
            case DataPackageInitState.Initialized:
                throw new InvalidOperationException(
                    "invalid injection: this type package has not been include when calling 'JLib.DataGeneration.DataPackageExtensions.IncludeDataPackages'.");
            case DataPackageInitState.Initializing:
                break;
            default:
                throw new IndexOutOfRangeException(nameof(packageManager.InitState));
        }

        foreach (var propertyInfo in GetType().GetProperties())
            packageManager.SetIdPropertyValue(this, propertyInfo);
    }
}