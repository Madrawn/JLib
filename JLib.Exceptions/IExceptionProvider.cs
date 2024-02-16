namespace JLib.Exceptions;

/// <summary>
/// enables a classes, like validators, to be added as children to the exception builder without having to pass it down to them<br/>
/// <see cref="GetException"/> will be evaluated lazily when <see cref="ExceptionBuilder.GetException"/> of the parent is called<br/>
/// <seealso cref="ExceptionBuilder.AddChild"/>
/// </summary>
public interface IExceptionProvider
{
    /// <summary>
    /// builds the exception based on the current builder state.
    /// </summary>
    /// <returns>null, if no error occured</returns>
    Exception? GetException();

    /// <summary>
    /// indicates, whether this <see cref="ExceptionBuilder"/> or one of its Children has any errors
    /// </summary>
    bool HasErrors();
}