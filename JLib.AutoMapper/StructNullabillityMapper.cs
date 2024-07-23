using AutoMapper;

namespace JLib.AutoMapper;

/// <summary>
/// contains a map from bool? to bool with null => false
/// </summary>
public class StructNullabilityMapper : Profile
{
    /// <summary>
    /// Creates the instance
    /// </summary>
    public StructNullabilityMapper()
    {
        CreateMap<bool?, bool>().ConvertUsing(x => x ?? false);
    }
}