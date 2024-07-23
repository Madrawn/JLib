
using JLib.Reflection;

namespace JLib.Configuration;

/// <summary>
/// JLib.Configuration Type package, containing all referenced assemblies
/// </summary>
[TypePackageProvider]
public static class JLibConfigurationTp
{
    /// <summary>
    /// the instance of this type package
    /// </summary>
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibConfigurationTp).Assembly);
}
