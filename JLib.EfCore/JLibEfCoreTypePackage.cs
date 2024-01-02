using JLib.Reflection;

namespace JLib.EfCore;
public static class JLibEfCoreTypePackage
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibEfCoreTypePackage).Assembly);
}
