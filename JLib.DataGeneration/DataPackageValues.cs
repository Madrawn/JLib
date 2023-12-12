using System.Reflection;
using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.DataGeneration;

public static class DataPackageValues
{
    /// <summary>
    /// the name of the property
    /// </summary>
    public record IdName(string Value) : StringValueType(Value)
    {
        public IdName(PropertyInfo property) : this(property.Name)
        { }
    }

    /// <summary>
    /// the name of the dataPackage
    /// </summary>
    public record IdGroupName(string Value) : StringValueType(Value)
    {
        public IdGroupName(Type dataPackage)
            : this(dataPackage.FullClassName())
        { }
        public IdGroupName(DataPackage dataPackage)
            : this(dataPackage.GetType())
        { }
        public IdGroupName(PropertyInfo property)
            : this(property.ReflectedType ?? throw new Exception("reflected type is null"))
        { }
    }

    public record IdIdentifier(IdGroupName IdGroupName, IdName IdName)
    {
        public IdIdentifier(PropertyInfo property) : this(new(property), new(property))
        { }
    }
}