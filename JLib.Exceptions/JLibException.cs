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