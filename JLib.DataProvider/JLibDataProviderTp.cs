
using JLib.Reflection;

namespace JLib.DataProvider;
[TypePackageProvider]
public static class JLibDataProviderTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDataProviderTp).Assembly);
}
