
using JLib.Reflection;

namespace JLib.DependencyInjection;
public static class JLibDependencyInjectionTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDependencyInjectionTp).Assembly);
}
