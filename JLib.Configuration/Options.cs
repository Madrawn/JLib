using System.Reflection;
using JLib.Exceptions;
using JLib.Reflection;
using JLib.ValueTypes;
using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.Configuration;

public record ConfigSectionName(string Value) : StringValueType(Value)
{
    public static implicit operator ConfigSectionName(string value) => new(value);
}
[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigSectionNameAttribute : Attribute
{
    public ConfigSectionNameAttribute(string sectionName)
    {
        SectionName = sectionName;
    }

    public ConfigSectionName SectionName { get; }
}

[HasAttribute(typeof(ConfigSectionNameAttribute)), IsClass, NotAbstract]
public sealed record ConfigurationSectionType(Type Value) : TypeValueType(Value), IPostNavigationInitializedType
{
    public ConfigSectionName SectionName { get; private set; } = null!;

    public void Initialize(ITypeCache cache, IExceptionBuilder exceptions)
        => SectionName = Value.GetCustomAttribute<ConfigSectionNameAttribute>()?.SectionName
                         ?? throw NewInvalidTypeException("sectionName not found");
}