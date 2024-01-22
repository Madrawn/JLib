using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;
using static JLib.Reflection.Attributes.TvtFactoryAttributes;

namespace JLib.DataGeneration;

[IsDerivedFrom(typeof(DataPackage)), NotAbstract]
public record DataPackageType(Type Value) : TypeValueType(Value);

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
                throw new ArgumentOutOfRangeException(nameof(packageManager.InitState));
        }


        foreach (var propertyInfo in GetType().GetProperties())
        {
            if (propertyInfo.GetMethod?.IsPublic is not true)
                continue;
            if (propertyInfo.CanWrite is false)
                throw new(propertyInfo.DeclaringType?.FullClassName() + "." + propertyInfo.Name +
                          " can not be written");
            if (!propertyInfo.IsInit())
                throw new(propertyInfo.DeclaringType?.FullClassName() + "." + propertyInfo.Name +
                          " set method must be init");
            packageManager.SetIdPropertyValue(this, propertyInfo);
        }
    }
}