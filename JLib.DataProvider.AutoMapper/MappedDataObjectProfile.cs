using System.ComponentModel.DataAnnotations;
using AutoMapper;
using JLib.Reflection;
using Microsoft.Extensions.Logging;

namespace JLib.DataProvider.AutoMapper;

/// <summary>
/// to make a property required for mapping, add the <see cref="RequiredAttribute"/> to it or add the required keyword on .net7 or higher
/// </summary>
public class MappedDataObjectProfile : Profile
{
    public MappedDataObjectProfile(ITypeCache cache, ILogger<MappedDataObjectProfile> logger)
    {
        logger.LogDebug("        Creating DataObjectMaps");
        foreach (var mapInfo in cache.All<IMappedDataObjectType>()
                     .Where(x => !x.HasCustomAutoMapperProfile)
                     .SelectMany(tvt => tvt.MappingInfo)
                     .Where(mapInfo =>
                         !mapInfo.Source.HasCustomAutoMapperProfile
                         && !mapInfo.Destination.HasCustomAutoMapperProfile)
                     .ToArray()
                )
        {
            var (source, destination, _) = mapInfo;
            CreateMap(source.Value, destination.Value);
        }
    }
}