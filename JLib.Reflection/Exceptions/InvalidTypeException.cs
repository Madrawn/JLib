using JLib.Exceptions;

namespace JLib.Reflection;

/// <summary>
/// Indicates, that a type has not been defined correctly.
/// </summary>
public sealed class InvalidTypeException : InvalidSetupException
{
    /// <summary>
    /// Gets the <see cref="ITypeValueType"/>s <see cref="Type"/> associated with the exception.
    /// </summary>
    public Type TvtType { get; }

    /// <summary>
    /// Gets the <see cref="ITypeValueType.Value"/> of the given <see cref="TvtType"/> instance associated with the exception.
    /// </summary>
    public Type Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTypeException"/> class with the specified TvtType, Value, and message.
    /// </summary>
    /// <param name="tvtType">The TvtType associated with the exception.</param>
    /// <param name="value">The Value associated with the exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public InvalidTypeException(Type tvtType, Type value, string message) : base(message)
    {
        TvtType = tvtType;
        Value = value;
        Data[nameof(TvtType)] = tvtType;
        Data[nameof(Value)] = value;
    }
}
