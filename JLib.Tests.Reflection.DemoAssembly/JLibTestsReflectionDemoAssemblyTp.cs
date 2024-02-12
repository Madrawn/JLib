
using JLib.Reflection;

namespace JLib.Tests.Reflection.DemoAssembly;
public static class JLibTestsReflectionDemoAssemblyTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibTestsReflectionDemoAssemblyTp).Assembly);
}
