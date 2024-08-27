using System.Text.Json;
using System.Text.Json.Nodes;

namespace JLib.Helper;

public static class ExceptionHelper
{
    /// <summary>
    /// Throws the <paramref name="exception"/> if it is not null.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> to be thrown.</param>
    public static void Throw(this Exception? exception)
    {
        if (exception is not null)
            throw exception;
    }

    /// <summary>
    /// recursively flattens the given <see cref="Exception"/> and returns them as a flat list, including all nested parents and all <see cref="Exception.InnerException"/> as separate entries.
    /// </summary>
    public static IEnumerable<Exception> FlattenAll(this Exception? exception) 
        => exception switch
        {
            AggregateException aggregate => 
                aggregate.InnerExceptions.SelectMany(FlattenAll)
                .Prepend(exception),
            not null => FlattenAll(exception.InnerException).Prepend(exception),
            _ => Enumerable.Empty<Exception>()
        };
}
