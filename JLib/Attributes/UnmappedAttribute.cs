using JLib.AutoMapper;

namespace JLib.Attributes;
/// <summary>
/// the <see cref="ValueTypeProfile"/> will not create a profile from <see cref="ValueTypeType{T}.Value"/> to the <see cref="ValueTypeType{T}"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class UnmappedAttribute : Attribute
{
}


public interface ICustomProfileAttribute
{
    public Type CustomProfile { get; }
}
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CustomProfileAttribute : Attribute, ICustomProfileAttribute
{
    public CustomProfileAttribute(Type customProfile)
    {
        CustomProfile = customProfile;
    }
    public Type CustomProfile { get; }
}

#if NET7_0_OR_GREATER
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

#endif
