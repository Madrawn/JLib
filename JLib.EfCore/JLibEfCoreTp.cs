
using JLib.Reflection;

namespace JLib.EfCore;
public static class JLibEfCoreTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibEfCoreTp).Assembly);
}
