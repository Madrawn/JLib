using JLib.Exceptions;

namespace JLib.Helper;
public static class ExceptionHelper
{
    public static void ThrowExceptionIfNotEmpty<T>(this IEnumerable<T> errors, string message)
        where T : Exception
    {
        var mat = errors.Cast<Exception>().ToArray();
        if (mat.Any())
        {
            var e = new JLibAggregateException(message, mat);
            throw e;
        }
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

    /// <summary>
    /// throws the <paramref name="exception"/> if it is not null
    /// </summary>
    /// <param name="exception"></param>
    public static void Throw(this Exception? exception)
    {
        if(exception is not null)
            throw exception;
    }
}
