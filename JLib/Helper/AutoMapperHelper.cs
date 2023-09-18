using AutoMapper;

namespace JLib.Helper;
public static class AutoMapperHelper
{
    public static void AddProfiles(this IMapperConfigurationExpression builder, ITypeCache typeCache){
        builder.AddProfiles(typeCache.All<Types.AutoMapperProfileType>().Select(p => p.Create(typeCache)));
    }
}
