using JLib.Reflection;

namespace JLib.DataGeneration.Examples;

public static class JLibDataGenerationExamplesTypePackage
{
    public static ITypePackage Instance { get; } =
        TypePackage.Get(typeof(JLibDataGenerationExamplesTypePackage).Assembly)
            .Combine(JLibDataGenerationTp.Instance);
}