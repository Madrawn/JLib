
using JLib.Reflection;

namespace JLib.DataProvider.InMemory;
public static class JLibDataProviderInMemoryTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDataProviderInMemoryTp).Assembly);
}
