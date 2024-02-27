
using JLib.Reflection;

namespace JLib.DataGeneration;
[TypePackageProvider]
public static class JLibDataGenerationTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDataGenerationTp).Assembly);
}
