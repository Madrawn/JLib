using JLib.Helper;

namespace JLib.HotChocolate.Reflection;


public class TypeGdo
{
    public string Name => _src.Name;

    public List<TypeGdo>? TypeArguments
        => _src.IsGenericType
        ? _src.GetGenericArguments().Select(x => new TypeGdo(x)).ToList()
        : null;

    public List<TypeGdo> ImplementedInterfaces
        => _src.GetInterfaces().Select(x => new TypeGdo(x)).ToList();
    public string? Namespace => _src.Namespace;
    public string FullClassName(bool includeNamespace = false) => _src.FullClassName(includeNamespace);
    private readonly Type _src;

    public TypeGdo(Type src)
    {
        _src = src;
    }
}
