using JLib.Helper;

namespace JLib.Exceptions;

/// <summary>
/// an <see cref="AggregateException"/> with the <see cref="AggregateException.Message"/> containing all <see cref="AggregateException.InnerExceptions"/> as tree
/// </summary>
public class JLibAggregateException : AggregateException
{
    public string UserMessage { get; }

    public JLibAggregateException(string userMessage, Exception[] content) : base(userMessage, content)
    {
        UserMessage = userMessage;
        _message = new(this.GetTreeInfo);
    }

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