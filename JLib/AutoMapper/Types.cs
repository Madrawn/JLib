using AutoMapper;
using JLib.FactoryAttributes;
using JLib.Helper;
using JLib.Reflection;

namespace JLib.AutoMapper;

[TvtFactoryAttributes.IsDerivedFrom(typeof(Profile)), TvtFactoryAttributes.NotAbstract]
public record AutoMapperProfileType(Type Value) : TypeValueType(Value)
{
    private static readonly Type[] CtorParamArray = new[] { typeof(ITypeCache) };
    public Profile Create(ITypeCache typeCache)
        => Value.GetConstructor(Array.Empty<Type>())
               ?.Invoke(null).As<Profile>()
           ?? Value.GetConstructor(CtorParamArray)
               ?.Invoke(new object[] { typeCache })
               .As<Profile>()
           ?? throw new InvalidOperationException($"Instantiation of {Name} failed.");
}