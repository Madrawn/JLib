using JLib.Reflection;

namespace JLib.HotChocolate;
public static class JLibHotChocolateTypePackage
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibHotChocolateTypePackage).Assembly);
}
