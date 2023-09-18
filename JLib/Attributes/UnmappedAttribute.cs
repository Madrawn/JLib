using AutoMapper;
using JLib.AutoMapper;

namespace JLib.Attributes;
/// <summary>
/// the <see cref="ValueTypeProfile"/> will not create a profile from <see cref="ValueType{T}.Value"/> to the <see cref="ValueType{T}"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class UnmappedAttribute : Attribute
{
}


public interface ICustomProfileAttribute
{
    public Type CustomProfile { get; }
}
/// <summary>
/// marks the type as <see cref="TypeValueType.HasCustomAutoMapperProfile"/> = true, which should remove it from all automated profiles and register the <see cref="Profile"/> of <typeparamref name="T"/> to autoMapper
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CustomProfileAttribute<T> : Attribute, ICustomProfileAttribute
    where T : Profile
{
    public Type CustomProfile => typeof(T);
}


