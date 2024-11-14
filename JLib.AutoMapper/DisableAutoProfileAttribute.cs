using JLib.Reflection;

namespace JLib.AutoMapper;

/// <summary>
/// when added to a type, no maps from and to this type should be created. This might or might not be implemented by a given feature.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DisableAutoProfileAttribute : Attribute, IDisableAutoProfileAttribute
{
}
