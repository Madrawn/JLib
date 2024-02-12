namespace JLib.Reflection;

/// <summary>
/// indicates, that the decorated type provides the type package for the assembly.<br/>
/// must only be used once per assembly
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TypePackageProviderAttribute : Attribute { }