
using JLib.Reflection;

namespace JLib;
public static class JLibTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibTp).Assembly);
}
