using System.Diagnostics.CodeAnalysis;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

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
        var res = $"{property.DeclaringType?.FullClassName()}.{property.Name}";
        if (property.DeclaringType != property.ReflectedType)
            res = $"{property.ReflectedType?.FullClassName()}:" + res;
        return res;
    }

    protected DataPackage(IIdRegistry idRegistry)
    {
        foreach (var propertyInfo in GetType().GetProperties())
        {
            if (!propertyInfo.PropertyType.IsAssignableTo<GuidValueType>())
                continue;
            if (propertyInfo.GetMethod?.IsPublic is not true)
                continue;
            if (propertyInfo.CanWrite is false)
                throw new(propertyInfo.DeclaringType?.FullClassName() + "." + propertyInfo.Name +
                          " can not be written");
            if (propertyInfo.SetMethod?.IsPublic is true)
                throw new(propertyInfo.DeclaringType?.FullClassName() + "." + propertyInfo.Name +
                          " set method must be protected");
            idRegistry.SetIdPropertyValue(propertyInfo);
        }
    }
}
