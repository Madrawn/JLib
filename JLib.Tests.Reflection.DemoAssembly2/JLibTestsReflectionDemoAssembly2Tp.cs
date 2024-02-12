
using JLib.Reflection;

namespace JLib.Tests.Reflection.DemoAssembly2;
public static class JLibTestsReflectionDemoAssembly2Tp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibTestsReflectionDemoAssembly2Tp).Assembly);
}
