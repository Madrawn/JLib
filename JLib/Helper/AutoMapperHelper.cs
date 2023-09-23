using AutoMapper;
using Serilog;

namespace JLib.Helper;
public static class AutoMapperHelper
{
    public static void AddProfiles(this IMapperConfigurationExpression builder, ITypeCache typeCache)
    {
        Log.Information("Loading AutoMapper Profiles");
        builder.AddProfiles(typeCache.All<AutoMapperProfileType>().Select(p =>
        {
            Log.Debug("    Loading {profile}", p.Name);
            return p.Create(typeCache);
        }));
    }
}
