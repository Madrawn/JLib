using System.ComponentModel.DataAnnotations;
using System.Reflection;
#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using JLib.Attributes;
using JLib.Data;
using JLib.Helper;
using Serilog;

namespace JLib.AutoMapper;
/// <summary>
/// to make a property required for mapping, add the <see cref="RequiredAttribute"/> to it or add the required keyword on .net7 or higher
/// </summary>
public class MappedDataObjectProfile : Profile
{
    public MappedDataObjectProfile(ITypeCache cache)
    {
        Log.ForContext<MappedDataObjectProfile>().Debug("        Creating DataObjectMaps");
        foreach (var mappedDataObject in cache.All<IMappedDataObjectType>()
                     .Where(mdo => mdo is { SourceEntity: not null, HasCustomAutoMapperProfile: false }))
        {
            if (mappedDataObject.HasCustomAutoMapperProfile)
            {
                Log.ForContext<MappedDataObjectProfile>().Verbose("        Skipping {mdo}: it has a custom profile", mappedDataObject.Value.Name);
                continue;
            }
            if (mappedDataObject.Value.HasCustomAttribute<UnmappedAttribute>())
            {
                Log.ForContext<MappedDataObjectProfile>().Verbose("        Skipping {mdo}: it has the Unmapped attribute", mappedDataObject.Value.Name);
                continue;
            }
            Log.ForContext<MappedDataObjectProfile>().Verbose("        creating map from {mappedDataObject} to {mappedEntity} ", mappedDataObject.SourceEntity.Name, mappedDataObject.Name);
            var map = base.CreateMap(mappedDataObject.SourceEntity.Value, mappedDataObject.Value);
            var mdoProps = mappedDataObject.Value.GetProperties();
            var srcProps = mappedDataObject.SourceEntity.Value.GetProperties().ToDictionary(x => x.Name);
            var prefix = mappedDataObject.PropertyPrefix?.Value ?? "";
            foreach (var mdoProp in mdoProps)
            {
                var srcProp =
                    srcProps.GetValueOrDefault(mdoProp.GetCustomAttribute<SourceMemberAttribute>()?.Name ?? "")
                    ?? srcProps.GetValueOrDefault(prefix + mdoProp.Name)
                    ?? srcProps.GetValueOrDefault(mdoProp.Name);
                if (srcProp is null)
                {
                    Log.ForContext<MappedDataObjectProfile>().Error("            could not map property {commandEntity}.{property} to {mappedDataObject} ",
                        mappedDataObject.Name, mdoProp.Name, mappedDataObject.Name);
                    continue;
                }

                Log.ForContext<MappedDataObjectProfile>().Verbose("            mapping from Property {cmdProp,-20} to {xrmProp} ",
                    mdoProp.Name, srcProp.Name);

                map.ForMember(mdoProp.Name, o => o.MapFrom(srcProp.Name));
            }

            if (mappedDataObject.ReverseMap)
                map.ReverseMap();
        }
    }
}