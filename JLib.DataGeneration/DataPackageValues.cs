using System.ComponentModel;
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
        public IdGroupName(Type type)
            : this(type.FullClassName())
        { }
        internal IdGroupName(DataPackage dataPackage)
            : this(dataPackage.GetType())
        { }

        internal IdGroupName(PropertyInfo property)
            : this(
                (property.DeclaringType != property.ReflectedType
                    ? property.ReflectedType?.FullClassName() + ":"
                    : "")
                + property.DeclaringType?.FullClassName()
            )
        { }

    }

    public record IdIdentifier(IdGroupName IdGroupName, IdName IdName)
    {
        public IdIdentifier(PropertyInfo property) : this(new(property), new(property))
        { }
        public override string ToString() => IdGroupName.Value + "." + IdName.Value;

    }
}