using AutoMapper;
using JLib.Reflection;
using Microsoft.Extensions.Logging;

namespace JLib.AutoMapper;

/// <summary>
/// Contains Extension methods for the AutoMapper package
/// </summary>
public static class AutoMapperHelper
{
    /// <summary>
    /// Adds all <see cref="Profile"/>s found by the <paramref name="typeCache"/> to the <paramref name="builder"/><br/>
    /// The <seealso cref="AutoMapperProfileType"/> is used to find <see cref="Profile"/>s
    /// </summary>
    public static void AddProfiles(this IMapperConfigurationExpression builder, ITypeCache typeCache, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(typeof(AutoMapperHelper));
        builder.AddProfiles(typeCache.All<AutoMapperProfileType>().Where(x => x.Value.IsGenericTypeDefinition is false).Select(p =>
        {
            logger.LogDebug("    Loading {profile}", p.Name);
            return p.Create(typeCache, loggerFactory);
        }));
    }
}