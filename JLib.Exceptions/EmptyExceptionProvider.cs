namespace JLib.Exceptions;

/// <summary>
/// an empty <see cref="IExceptionProvider"/> Instance which contains no errors.
/// </summary>
public sealed class EmptyExceptionProvider : IExceptionProvider
{
    private EmptyExceptionProvider() { }

    /// <summary>
    /// the instance of this singleton
    /// </summary>
    public static IExceptionProvider Instance { get; } = new EmptyExceptionProvider();
    /// <summary>
    /// <inheritdoc cref="IExceptionProvider.GetException"/>
    /// </summary>
    /// <returns></returns>
    public Exception? GetException() => null;

    /// <summary>
    /// <inheritdoc cref="IExceptionProvider.HasErrors"/>
    /// </summary>
    /// <returns></returns>
    public bool HasErrors() => false;
}