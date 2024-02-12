
using JLib.Reflection;

namespace JLib.DataGeneration;
public static class JLibDataGenerationTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDataGenerationTp).Assembly);
}
