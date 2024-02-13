
using JLib.Reflection;

namespace JLib.Reflection.DependencyInjection;
public static class JLibReflectionDependencyInjectionTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibReflectionDependencyInjectionTp).Assembly);
}
