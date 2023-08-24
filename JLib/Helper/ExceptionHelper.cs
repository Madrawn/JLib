using JLib.Exceptions;

namespace JLib.Helper;
public static class ExceptionHelper
{
    public static void ThrowIfNotEmpty<T>(this IEnumerable<T> errors, string message)
        where T : Exception
    {
        var mat = errors.Cast<Exception>().ToArray();
        if (mat.Any())
        {
            throw new JLibAggregateException(message, mat);
        }
    }
}
