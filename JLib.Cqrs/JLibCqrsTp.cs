
using JLib.Reflection;

namespace JLib.Cqrs;
public static class JLibCqrsTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibCqrsTp).Assembly);
}
