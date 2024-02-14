using AutoMapper;
using JLib.Reflection;
using Microsoft.Extensions.Logging;

namespace JLib.AutoMapper;

public static class AutoMapperHelper
{
    public static void AddProfiles(this IMapperConfigurationExpression builder, ITypeCache typeCache, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(typeof(AutoMapperHelper));
        builder.AddProfiles(typeCache.All<AutoMapperProfileType>().Select(p =>
        {
            logger.LogDebug("    Loading {profile}", p.Name);
            return p.Create(typeCache, loggerFactory);
        }));
    }
}