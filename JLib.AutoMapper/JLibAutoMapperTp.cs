
using JLib.Reflection;

namespace JLib.AutoMapper;
/// <summary>
/// Contains the <inheritdoc cref="Instance"/>
/// </summary>
[TypePackageProvider]
public static class JLibAutoMapperTp
{
    /// <summary>
    /// <see cref="TypePackage"/> for the JLib.AutoMapper assembly
    /// </summary>
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibAutoMapperTp).Assembly);
}
