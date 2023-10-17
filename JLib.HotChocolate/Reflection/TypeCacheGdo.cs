using System.Reflection;

namespace JLib.HotChocolate.Reflection;
public class TypeCacheGdo
{
    private readonly ITypeCache _typeCache;

    public TypeCacheGdo(ITypeCache typeCache)
    {
        _typeCache = typeCache;
        All = _typeCache.All<TypeValueType>().Select(x => new TypeValueTypeGdo(x)).ToArray();
    }

    [UseFiltering]
    public TypeValueTypeGdo[] All { get; }

    [UseFiltering]
    public TypeValueTypeGroupGdo[] ByTypeValueType
        => _typeCache.All<TypeValueType>().GroupBy(x => x.GetType(), tvtt => new TypeGdo(tvtt.Value))
            .Select(g => new TypeValueTypeGroupGdo(new(g.Key), g.ToArray())).ToArray();

    [UseFiltering]
    public AssemblyTypeGroupGdo[] ByAssembly
        => _typeCache.All<TypeValueType>().ToLookup(x => x.Value.Assembly)
            .Select(g => new AssemblyTypeGroupGdo(g.Key, g.ToArray()))
            .ToArray();

    [UseFiltering]
    public TypeGdo[] KnownTypeValueTypes => _typeCache.KnownTypeValueTypes.Select(t => new TypeGdo(t)).ToArray();

    [UseFiltering]
    public AssemblyGdo[] IncludedAssemblies
        => _typeCache.All<TypeValueType>().Select(x => x.Value.Assembly).ToHashSet().Select(a => new AssemblyGdo(a)).ToArray();
}

public class AssemblyTypeGroupGdo
{
    public AssemblyGdo Assembly { get; }
    [UseFiltering]
    public TypeValueTypeGroupGdo[] Types { get; }

    public AssemblyTypeGroupGdo(Assembly assembly, TypeValueType[] types)
    {
        Assembly = new(assembly);
        Types = types.GroupBy(t => t.GetType())
            .Select(g => new TypeValueTypeGroupGdo(new(g.Key), g.Select(t => new TypeGdo(t.Value)).ToArray()))
            .ToArray();
    }
}