
using JLib.Reflection;

namespace JLib.SystemTextJson;
[TypePackageProvider]
public static class JLibSystemTextJsonTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibSystemTextJsonTp).Assembly);
}
