
using JLib.Reflection;

namespace JLib.ValueTypes.Mapping;
[TypePackageProvider]
public static class JLibValueTypesMappingTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibValueTypesMappingTp).Assembly);
}
