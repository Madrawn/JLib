
using JLib.Reflection;

namespace JLib.HotChocolate;
[TypePackageProvider]
public static class JLibHotChocolateTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibHotChocolateTp).Assembly);
}
