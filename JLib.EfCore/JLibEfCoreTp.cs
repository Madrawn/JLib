
using JLib.Reflection;

namespace JLib.EfCore;
[TypePackageProvider]
public static class JLibEfCoreTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibEfCoreTp).Assembly);
}
