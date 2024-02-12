
using JLib.Reflection;

namespace JLib.AutoMapper;


[TypePackageProvider]
public static class JLibAutoMapperTypePackage
{
    public static ITypePackage Instance { get; } 
        = TypePackage.Get(typeof(JLibAutoMapperTypePackage).Assembly);
}
