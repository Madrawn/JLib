using JLib.Exceptions;

namespace JLib.Helper;
public static class ExceptionHelper
{
    public static void RaiseExceptionIfNotEmpty<T>(this IEnumerable<T> errors, string message)
        where T : Exception
    {
        var mat = errors.Cast<Exception>().ToArray();
        if (mat.Any())
            throw new JLibAggregateException(message, mat);
    }
    public static Exception? GetExceptionIfNotEmpty<T>(this IEnumerable<T> errors, string message)
        where T : Exception
    {
        var mat = errors.Cast<Exception>().ToArray();
        return mat.Any() 
            ? new JLibAggregateException(message, mat)
            : null;
    }
    public static void AddIfNotEmpty<T>(this IEnumerable<T> errors, string message, IList<Exception> masterExceptionList)
        where T : Exception
    {
        var mat = errors.Cast<Exception>().ToArray();
        if (mat.Any())
        {
            masterExceptionList.Add(new JLibAggregateException(message, mat));
        }
    }
}
