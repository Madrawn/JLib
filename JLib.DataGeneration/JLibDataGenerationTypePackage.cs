using JLib.Reflection;

namespace JLib.DataGeneration;
/// <summary>
/// contains all types of the JLib.DataGeneration assembly and its dependencies
/// </summary>
public static class JLibDataGenerationTypePackage
{
    public static ITypePackage Instance { get; } = 
        TypePackage.Get(typeof(JLibDataGenerationTypePackage).Assembly)
            .Combine(JLibTypePackage.Instance);
}
