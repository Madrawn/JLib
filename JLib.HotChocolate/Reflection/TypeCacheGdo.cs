using System.Reflection;
using HotChocolate;
using JLib.Helper;

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
    public TypeValueTypeGroupGdo[] ByTypeValueType([Service] IGraphQlReflectionEndpointCache gdoCache)
        => _typeCache.All<TypeValueType>().GroupBy(x => x.GetType(), gdoCache.ToGdo)
            .Select(g => new TypeValueTypeGroupGdo(new(g.Key), g.WhereNotNull().ToArray())).ToArray();

    [UseFiltering]
    public AssemblyTypeGroupGdo[] ByAssembly([Service] IGraphQlReflectionEndpointCache gdoCache)
        => _typeCache.All<TypeValueType>().ToLookup(x => x.Value.Assembly)
            .Select(g => new AssemblyTypeGroupGdo(g.Key, g.ToArray(), gdoCache))
            .ToArray();

    [UseFiltering]
    public TypeGdo[] KnownTypeValueTypes([Service] IGraphQlReflectionEndpointCache gdoCache)
        => _typeCache.KnownTypeValueTypes.Select(gdoCache.ToGdo).WhereNotNull().ToArray();

    [UseFiltering]
    public AssemblyGdo[] IncludedAssemblies
        => _typeCache.All<TypeValueType>()
            .Select(x => x.Value.Assembly)
            .ToHashSet()
            .Select(a => new AssemblyGdo(a))
            .ToArray();
}

public class AssemblyTypeGroupGdo
{
    public AssemblyGdo Assembly { get; }
    [UseFiltering]
    public TypeValueTypeGroupGdo[] Types { get; }

    public AssemblyTypeGroupGdo(Assembly assembly, TypeValueType[] types, IGraphQlReflectionEndpointCache gdoCache)
    {
        Assembly = new(assembly);
        Types = types.GroupBy(t => t.GetType())
            .Select(g => new TypeValueTypeGroupGdo(
                new(g.Key), g.Select(gdoCache.ToGdo).WhereNotNull().ToArray()))
            .ToArray();
    }
}