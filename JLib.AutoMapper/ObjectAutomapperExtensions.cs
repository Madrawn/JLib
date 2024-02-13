using AutoMapper;

namespace JLib.AutoMapper;
public static class ObjectAutomapperExtensions
{
    public static T MapTo<T>(this object obj, IMapper mapper)
        => mapper.Map<T>(obj);
}
