using AutoMapper;

namespace JLib.AutoMapper;
/// <summary>
/// Contains Extension methods for the AutoMapper package
/// </summary>
public static class ObjectAutomapperExtensions
{
    /// <summary>
    /// Maps the <paramref name="obj"/> to a new instance of <typeparamref name="T"/> using the given <paramref name="mapper"/>
    /// </summary>
    public static T MapTo<T>(this object obj, IMapper mapper)
        => mapper.Map<T>(obj);
}
