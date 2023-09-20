using System.ComponentModel.DataAnnotations;
#if NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using System.Runtime.InteropServices;
using AutoMapper;
using JLib.Helper;
using Serilog;

namespace JLib.AutoMapper;
/// <summary>
/// to make a property required for mapping, add the <see cref="RequiredAttribute"/> to it or add the required keyword on .net7 or higher
/// </summary>
public class EntityProfile : Profile
{
    public EntityProfile(ITypeCache cache)
    {
        Log.Debug("        Adding GraphQlDataObject Map");
        foreach (var gdo in cache.All<Types.GraphQlDataObject>()
                     .Where(gdo => gdo is { CommandEntity: not null, HasCustomAutoMapperProfile: false }))
        {
            Log.Verbose("            Mapping from {cmd} to {gdo}", gdo.CommandEntity!.Name, gdo.Name);
            CreateMap(gdo.CommandEntity!.Value, gdo.Value);
        }

        Log.Debug("        Adding GraphQlMutationParameters");
        foreach (var gmp in cache.All<Types.GraphQlMutationParameter>()
                     .Where(gdo => gdo is { CommandEntity: not null, HasCustomAutoMapperProfile: false }))
        {
            Log.Verbose("            Mapping from {cmd} to {gdo}", gmp.CommandEntity!.Name, gmp.CommandEntity.Name);
            var ceProps = gmp.CommandEntity!.Value.GetProperties();
            var gmpProps = gmp.Value.GetProperties();


            var mapper = CreateMap(gmp.Value, gmp.CommandEntity!.Value);


            // remove all properties which are missing in the mutation parameter and are not required
            var propsToIgnore = ceProps
                .ExceptBy(gmpProps.Select(pGmp => pGmp.Name), pCe => pCe.Name)
                .Where(ceProp => !ceProp.HasCustomAttribute<RequiredAttribute>()
#if NET7_0_OR_GREATER
                    && !ceProp.HasCustomAttribute<RequiredMemberAttribute>()
#endif
                );
            foreach (var prop in propsToIgnore)
            {
                mapper.ForMember(prop.Name, o => o.Ignore());
                Log.Verbose("            Adding {propName} to the ignore list", prop.Name);
            }
        }
    }
}