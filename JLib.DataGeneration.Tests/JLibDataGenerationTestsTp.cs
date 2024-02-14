
using JLib.Reflection;

namespace JLib.DataGeneration.Tests;
public static class JLibDataGenerationTestsTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDataGenerationTestsTp).Assembly);
}
