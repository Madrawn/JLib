using System.Collections;

namespace JLib.Exceptions;

/// <summary>
///     baseClass for all custom exceptions.
///     can be used to handle custom exceptions separate
/// </summary>
public abstract class JLibException : Exception
{
    /// <summary>
    /// <inheritdoc cref="Data"/>
    /// </summary>
    public sealed override IDictionary Data => base.Data;

    /// <summary>
    /// <inheritdoc cref="Exception()"/>
    /// </summary>
    protected JLibException() : this(null)
    {
    }
    /// <summary>
    /// <inheritdoc cref="Exception(string)"/>
    /// </summary>
    protected JLibException(string? message) : this(message, null)
    {
    }
    /// <summary>
    /// <inheritdoc cref="Exception(string, Exception)"/>
    /// </summary>
    protected JLibException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}