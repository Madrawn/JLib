using JLib.Reflection;

namespace JLib;

public static class JLibTypePackage
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibTypePackage).Assembly);
}