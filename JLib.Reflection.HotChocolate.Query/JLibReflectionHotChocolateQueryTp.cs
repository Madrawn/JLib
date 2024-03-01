
namespace JLib.Reflection.HotChocolate.Query;
[TypePackageProvider]
public static class JLibReflectionHotChocolateQueryTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibReflectionHotChocolateQueryTp).Assembly);
}
