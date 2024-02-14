
using JLib.Reflection;

namespace JLib.AutoMapper;
public static class JLibAutoMapperTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibAutoMapperTp).Assembly);
}
