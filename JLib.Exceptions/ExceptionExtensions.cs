using System.Text;

namespace JLib.Exceptions;
public static class ExceptionExtensions
{
    /// <summary>
    /// throws a <see cref="JLibAggregateException"/> with the given <paramref name="message"/> containing all <paramref name="exceptions"/> if <paramref name="exceptions"/> is not empty.
    /// </summary>
    /// <typeparam name="T">the exception type</typeparam>
    /// <param name="exceptions"></param>
    /// <param name="message"><see cref="JLibAggregateException.Message"/> of the to-be thrown exception</param>
    public static void ThrowExceptionIfNotEmpty<T>(this IEnumerable<T> exceptions, string message)
        where T : Exception
    {
        var mat = exceptions.Cast<Exception>().ToArray();

        if (!mat.Any())
            return;

        var e = new JLibAggregateException(message, mat);
        throw e;
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="exceptions"></param>
    /// <param name="message"><see cref="JLibAggregateException.Message"/> of the to-be thrown exception</param>
    /// <returns>a <see cref="JLibAggregateException"/> containing all <paramref name="exceptions"/> if <paramref name="exceptions"/> is not empty, otherwise it returns null</returns>
    public static Exception? GetExceptionIfNotEmpty<T>(this IEnumerable<T> exceptions, string message)
        where T : Exception
    {
        var mat = exceptions.Cast<Exception>().ToArray();
        return mat.Any()
            ? new JLibAggregateException(message, mat)
            : null;
    }
    /// <summary>
    /// adds all <paramref name="errors"/> bundled as <see cref="JLibAggregateException"/> to the <see cref="masterExceptionList"/> if <paramref name="errors"/> is not empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="errors"></param>
    /// <param name="message"><see cref="JLibAggregateException.Message"/> of the to-be thrown exception</param>
    /// <param name="masterExceptionList">the list the generated <see cref="JLibAggregateException"/> should be added to</param>
    public static void AddIfNotEmpty<T>(this IEnumerable<T> errors, string message,
        IList<Exception> masterExceptionList)
        where T : Exception
    {
        var mat = errors.Cast<Exception>().ToArray();
        if (mat.Any())
        {
            masterExceptionList.Add(new JLibAggregateException(message, mat));
        }
    }

    /// <summary>
    /// returns a string which visualized the exception as grouped tree
    /// </summary>
    public static string GetTreeInfo(this AggregateException exception)
    {
        return new StringBuilder()
            .Append("│  ")
            .AppendLine(exception is JLibAggregateException je ? je.UserMessage : exception.Message)
            .Append("├─")
            .AppendJoin(Environment.NewLine + "├─",
                exception.InnerExceptions
                    .ToLookup(ex => ex.GetType())
                    .OrderBy(group => group.Key.Name)
                    .Select(group =>
                        " " + group.Count() + " " + group.Key.Name + Environment.NewLine +
                        string.Join(Environment.NewLine,
                            group.OrderBy(ex => ex.Message)
                                .Select(ex =>
                                    (ex is NullReferenceException
                                        ? ex.ToString()
                                        : ex.Message.Replace(Environment.NewLine, Environment.NewLine + "│  "))
                                )
                        ) + Environment.NewLine
                    )
            )
            .ToString();
    }

}
