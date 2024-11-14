using System.Reflection;
using JLib.ValueTypes;

namespace JLib.Reflection;

/// <summary>
/// a <see cref="ValueType{T}"/> for <see cref="PropertyInfo"/>s
/// </summary>
/// <param name="Value"></param>
[Unmapped]
public abstract record PropertyInfoValueType(PropertyInfo Value) : ValueType<PropertyInfo>(Value)
{
    /// <summary>
    /// <inheritdoc cref="ITypeValueType.Name"/>
    /// </summary>
    public string Name => Value.Name;
}