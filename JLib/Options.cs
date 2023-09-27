using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JLib.Exceptions;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib;

public record ConfigSectionName(string Value) : StringValueType(Value)
{
    public static implicit operator ConfigSectionName(string value) => new(value);
}

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

    public void Initialize(IExceptionManager exceptions)
        => SectionName = Value.GetCustomAttribute<ConfigSectionNameAttribute>()?.SectionName
        ?? throw NewInvalidTypeException("sectionName not found");
}
