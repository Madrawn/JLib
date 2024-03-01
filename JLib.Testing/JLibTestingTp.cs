
using JLib.Reflection;

namespace JLib.Testing;
[TypePackageProvider]
public static class JLibTestingTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibTestingTp).Assembly);
}
