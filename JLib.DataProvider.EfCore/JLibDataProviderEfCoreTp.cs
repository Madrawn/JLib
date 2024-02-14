
using JLib.Reflection;

namespace JLib.DataProvider.EfCore;
public static class JLibDataProviderEfCoreTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDataProviderEfCoreTp).Assembly);
}
