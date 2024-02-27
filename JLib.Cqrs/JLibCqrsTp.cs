
using JLib.Reflection;

namespace JLib.Cqrs;
[TypePackageProvider]
public static class JLibCqrsTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibCqrsTp).Assembly);
}
