
namespace JLib.Reflection.DependencyInjection;
[TypePackageProvider]
public static class JLibReflectionDependencyInjectionTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibReflectionDependencyInjectionTp).Assembly);
}
