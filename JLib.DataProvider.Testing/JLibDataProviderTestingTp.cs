using JLib.Reflection;

namespace JLib.DataProvider.Testing;
[TypePackageProvider]
public static class JLibDataProviderTestingTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDataProviderTestingTp).Assembly);
}
