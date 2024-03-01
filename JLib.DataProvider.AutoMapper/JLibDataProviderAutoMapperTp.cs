
using JLib.Reflection;

namespace JLib.DataProvider.AutoMapper;
[TypePackageProvider]
public static class JLibDataProviderAutoMapperTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDataProviderAutoMapperTp).Assembly);
}
