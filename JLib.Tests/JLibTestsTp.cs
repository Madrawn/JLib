
using JLib.Reflection;

namespace JLib.Tests;
public static class JLibTestsTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibTestsTp).Assembly);
}
