using System.Reflection;

namespace JLib.HotChocolate.Reflection;

public class AssemblyGdo
{
    private readonly Assembly _assembly;
    public string? FullName => _assembly.FullName;

    public AssemblyGdo(Assembly assembly)
    {
        _assembly = assembly;
    }
}