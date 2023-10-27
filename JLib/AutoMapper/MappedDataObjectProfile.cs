using System.ComponentModel.DataAnnotations;
using AutoMapper;
using JLib.Data;
using JLib.Helper;
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
            var (source, destination, _, sourcePropertyResolver, destinationPropertyResolver) = mapInfo;
            var map = base.CreateMap(source.Value, destination.Value);
            var srcProps = source.Value.GetProperties();
            var dstProps = destination.Value.GetProperties();
            Log.ForContext<MappedDataObjectProfile>().Debug("          {Source} => {Destination}", source, destination);
            foreach (var dstProp in dstProps)
            {
                var resolvedDestinationPropertyName = destinationPropertyResolver.Aggregate(dstProp.Name,
                    (name, resolver) => resolver.GetComparisonString(name));
                var matchingProps = srcProps
                    .Where(srcProp =>
                        sourcePropertyResolver.Aggregate(srcProp.Name, (name, resolver) => resolver.GetComparisonString(name)) ==
                        resolvedDestinationPropertyName
                    ).ToArray();
                switch (matchingProps.Count())
                {
                    case 0:
                        Log.Warning(
                            "{DestinationType}.{DestinationProperty} has no matching property on {SourceType} using the following resolvers: source: {sourceResolver} destination: {destinationResolver} (result: {DestinationResolverResult})",
                            destination.Value.FullClassName(true),
                            dstProp.Name, source.Value.FullClassName(true),
                            sourcePropertyResolver,
                            destinationPropertyResolver,
                            resolvedDestinationPropertyName
                            );
                        continue;
                    case > 1:
                        Log.Warning(
                            "{DestinationType}.{DestinationProperty} has multiple matching properties on {SourceType}: {sourceProperties} using the following resolvers: resolvers: source: {sourceResolver} destination: {destinationResolver} (result: {DestinationResolverResult})",
                            destination.Value.FullClassName(true),
                            dstProp.Name, source.Value.FullClassName(true),
                            matchingProps.Select(p => p.Name).ToArray(),
                            sourcePropertyResolver,
                            destinationPropertyResolver,
                            resolvedDestinationPropertyName
                        );
                        continue;
                }

                var srcProp = matchingProps.Single();
                Log.ForContext<MappedDataObjectProfile>().Debug("            {SourcePropertyType} {SourceProperty} => {DestinationPropertyType} {DestinationProperty}",
                    srcProp.PropertyType.FullClassName(), srcProp.Name,
                    dstProp.PropertyType.FullClassName(), dstProp.Name);
                map.ForMember(dstProp.Name, b => b.MapFrom(srcProp.Name));
            }
        }
    }
}