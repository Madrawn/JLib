using AutoMapper;
using JLib.Reflection;
using Serilog;

namespace JLib.Helper;
public static class AutoMapperHelper
{
    public static void AddProfiles(this IMapperConfigurationExpression builder, ITypeCache typeCache)
    {
        Log.ForContext(typeof(AutoMapperHelper)).Information("Loading AutoMapper Profiles");
        builder.AddProfiles(typeCache.All<AutoMapperProfileType>().Select(p =>
        {
            Log.ForContext(typeof(AutoMapperHelper)).Debug("    Loading {profile}", p.Name);
            return p.Create(typeCache);
        }));
    }
}
