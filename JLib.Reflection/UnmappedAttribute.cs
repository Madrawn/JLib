namespace JLib.Reflection;

/// <summary>
/// Prevents TypeCache based profiles from generating maps from and to this type.<br/>
/// Profiles have to implement this manually
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class UnmappedAttribute : Attribute
{
}