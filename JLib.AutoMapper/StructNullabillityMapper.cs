using AutoMapper;

namespace JLib.AutoMapper;

public class StructNullabillityMapper : Profile
{
    public StructNullabillityMapper()
    {
        CreateMap<bool?, bool>().ConvertUsing(x => x ?? false);
    }
}