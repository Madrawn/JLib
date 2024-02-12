using AutoMapper;
using JLib.Helper;
using JLib.Reflection;
using JLib.Reflection.Attributes;

namespace JLib.AutoMapper;

[TvtFactoryAttribute.IsDerivedFrom(typeof(Profile)), TvtFactoryAttribute.NotAbstract]
public record AutoMapperProfileType(Type Value) : TypeValueType(Value)
{
    private static readonly Type[] CtorParamArray = new[] { typeof(ITypeCache) };

    public Profile Create(ITypeCache typeCache)
    {
        return Value.GetConstructor(Array.Empty<Type>())
                   ?.Invoke(null).As<Profile>()
               ?? Value.GetConstructor(CtorParamArray)
                   ?.Invoke(new object[] { typeCache })
                   .As<Profile>()
               ?? throw new InvalidOperationException($"Instantiation of {Name} failed.");
    }
}