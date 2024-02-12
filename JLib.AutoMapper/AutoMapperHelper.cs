using AutoMapper;
using JLib.Reflection;
using Microsoft.Extensions.Logging;

namespace JLib.AutoMapper;

public static class AutoMapperHelper
{
    public static void AddProfiles(this IMapperConfigurationExpression builder, ITypeCache typeCache, ILogger logger)
    {
        using var scope = logger.BeginScope("Adding AutoMapper profiles");
        builder.AddProfiles(typeCache.All<AutoMapperProfileType>().Select(p =>
        {
            logger.LogDebug("    Loading {profile}", p.Name);
            return p.Create(typeCache);
        }));
    }
}