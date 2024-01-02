using JLib.Reflection;

namespace JLib.Testing;
public static class JLibTestingTypePackage
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibTestingTypePackage).Assembly);
}
