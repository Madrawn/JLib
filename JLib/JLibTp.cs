
using JLib.Reflection;

namespace JLib;
/// <summary>
/// Contains the <inheritdoc cref="Instance"/>
/// </summary>
public static class JLibTp
{
    /// <summary>
    /// <see cref="TypePackage"/> for the JLib assembly
    /// </summary>
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibTp).Assembly);
}
