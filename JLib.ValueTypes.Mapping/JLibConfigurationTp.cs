
using JLib.Reflection;

namespace JLib.ValueTypes.Mapping;
[TypePackageProvider]
public static class JLibValueTypesMapping
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibValueTypesMapping).Assembly);
}
