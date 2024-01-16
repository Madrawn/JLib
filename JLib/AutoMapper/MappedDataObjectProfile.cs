using System.ComponentModel.DataAnnotations;
using AutoMapper;
using JLib.Data;
using JLib.Reflection;
using Serilog;
#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace JLib.AutoMapper;
/// <summary>
/// to make a property required for mapping, add the <see cref="RequiredAttribute"/> to it or add the required keyword on .net7 or higher
/// </summary>
public class MappedDataObjectProfile : Profile
{
    public MappedDataObjectProfile(ITypeCache cache)
    {
        Log.ForContext<MappedDataObjectProfile>().Debug("        Creating DataObjectMaps");
        foreach (var mapInfo in cache.All<IMappedDataObjectType>()
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