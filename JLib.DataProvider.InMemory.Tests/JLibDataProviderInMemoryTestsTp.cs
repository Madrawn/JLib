
using JLib.Reflection;

namespace JLib.DataProvider.InMemory.Tests;
public static class JLibDataProviderInMemoryTestsTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDataProviderInMemoryTestsTp).Assembly);
}
