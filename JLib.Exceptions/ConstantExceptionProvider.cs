namespace JLib.Exceptions;

/// <summary>
/// an <see cref="IExceptionProvider"/> which always returns the same <see cref="Exception"/>
/// </summary>
public sealed class ConstantExceptionProvider : IExceptionProvider
{
    private readonly Exception? _exception;

    /// <summary>
    /// creates a new <see cref="ConstantExceptionProvider"/> with the given <paramref name="exception"/>
    /// </summary>
    /// <param name="exception">the exception to be returned. <see langword="null"/> will make <see cref="HasErrors"/> to return false.</param>
    public ConstantExceptionProvider(Exception? exception)
    {
        _exception = exception;
    }

    /// <summary>
    /// <inheritdoc cref="IExceptionProvider.GetException"/>
    /// </summary>
    public Exception? GetException() => _exception;

    /// <summary>
    /// <inheritdoc cref="IExceptionProvider.HasErrors"/>
    /// </summary>
    public bool HasErrors() => _exception is null;
}