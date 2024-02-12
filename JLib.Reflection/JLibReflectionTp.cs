namespace JLib.Reflection;

[TypePackageProvider]
public static class JLibReflectionTp
{
    public static ITypePackage Instance { get; } 
        = TypePackage.Get(typeof(JLibReflectionTp).Assembly);
}