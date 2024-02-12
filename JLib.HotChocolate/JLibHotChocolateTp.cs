
using JLib.Reflection;

namespace JLib.HotChocolate;
public static class JLibHotChocolateTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibHotChocolateTp).Assembly);
}
