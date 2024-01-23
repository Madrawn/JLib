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
        {
        }
    }

    /// <summary>
    /// the name of the dataPackage
    /// </summary>
    public record IdGroupName(string Value) : StringValueType(Value)
    {
        public IdGroupName(Type type)
            : this(type.FullClassName(true))
        {
        }

        internal IdGroupName(DataPackage dataPackage)
            : this(dataPackage.GetType())
        {
        }

        private static string ExtractKey(PropertyInfo property)
        {
            if (property.DeclaringType is null)
                return "No declaring type found";
            if (property.ReflectedType is null)
                return "No reflected type found";

            if (property.DeclaringType == property.ReflectedType)
                return property.ReflectedType.FullClassName(true);

            var baseTypeTree = property.ReflectedType
                .GetBaseTypeTree()
                .TakeWhile(t => t != property.DeclaringType)
                .Append(property.DeclaringType)
                .Select(t=>t.FullClassName(true));

            return string.Join(":", baseTypeTree);
        }

        internal IdGroupName(PropertyInfo property) : this(ExtractKey(property))
        {
        }
    }

    public record IdIdentifier(IdGroupName IdGroupName, IdName IdName)
    {
        public IdIdentifier(PropertyInfo property) : this(new(property), new(property))
        {
        }

        public override string ToString() => $"[{IdGroupName.Value}].[{IdName.Value}]";
    }
}