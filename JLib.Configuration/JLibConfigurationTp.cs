
using JLib.Reflection;

namespace JLib.Configuration;
[TypePackageProvider]
public static class JLibConfigurationTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibConfigurationTp).Assembly);
}
