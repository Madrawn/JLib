using JLib.Reflection;

namespace JLib.DataGeneration;
public static class JLibDataGenerationTypePackage
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDataGenerationTypePackage).Assembly);
}
