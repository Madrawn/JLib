using System.Text;
using JLib.Helper;

namespace JLib.Exceptions;

/// <summary>
///     baseClass for all custom exceptions.
///     can be used to handle custom exceptions separate
/// </summary>
public abstract class JLibException : Exception
{
    protected JLibException() : this(null)
    {
    }

    protected JLibException(string? message) : this(message, null)
    {
    }

    protected JLibException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
///     baseClass for all custom exceptions.
///     can be used to handle custom exceptions separate
/// </summary>
public class JLibAggregateException : AggregateException
{
    public string UserMessage { get; }

    public JLibAggregateException(string userMessage, Exception[] content) : base(userMessage, content)
    {
        UserMessage = userMessage;
        _message = new(() => new StringBuilder()
            .Append("│  ")
            .AppendLine(userMessage)
            .Append("├─")
            .AppendJoin(Environment.NewLine + "├─",
                content
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
            .ToString());
    }

    public static void ThrowIfNotEmpty(string message, IEnumerable<Exception> content)
    {
        var ex = ReturnIfNotEmpty(message, content);
        if (ex is not null)
            throw ex;
    }

    public static void ThrowIfNotEmpty(string message, params Exception[] content)
        => ThrowIfNotEmpty(message, content.AsEnumerable());

    public static Exception? ReturnIfNotEmpty(string message, params Exception[] content)
        => content.Length switch
        {
            0 => null,
            _ => new JLibAggregateException(message, content)
        };

    public static Exception? ReturnIfNotEmpty(string message, IEnumerable<Exception?> content)
        => ReturnIfNotEmpty(message, content.WhereNotNull().ToArray());

    private readonly Lazy<string> _message;

    public override string Message
        => _message.Value;
}