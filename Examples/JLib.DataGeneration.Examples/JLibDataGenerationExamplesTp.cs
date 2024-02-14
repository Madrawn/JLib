
using JLib.Reflection;

namespace JLib.DataGeneration.Examples;
public static class JLibDataGenerationExamplesTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDataGenerationExamplesTp).Assembly);
}
