using System.Diagnostics;
using System.Reflection;
using AutoMapper;
using JLib.Helper;
using JLib.Reflection;
using Microsoft.Extensions.Logging;

namespace JLib.AutoMapper;

[TvtFactoryAttribute.IsDerivedFrom(typeof(Profile)), TvtFactoryAttribute.NotAbstract]
public record AutoMapperProfileType(Type Value) : TypeValueType(Value)
{
    public Profile Create(ITypeCache typeCache, ILoggerFactory loggerFactory)
    {
        var ctor = Value.GetConstructors().Single();

        var args = ctor.GetParameters().Select(p =>
            p.ParameterType == typeof(ITypeCache)
                ? typeCache.As<object>()
                : p.ParameterType == typeof(ILoggerFactory)
                    ? loggerFactory.As<object>()
                    : throw new InvalidOperationException(
                        $"unexpected ctor parameter {p.Name} of type {p.ParameterType.Name} in {Value.FullClassName()}")
        ).ToArray();

        return ctor.Invoke(args).As<Profile>()
               ?? throw new InvalidOperationException($"Instantiation of {Name} failed.");
    }
}