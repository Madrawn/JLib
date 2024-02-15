namespace JLib.Exceptions;

/// <summary>
/// enables a classes, like validators, to be added as children to the exception builder without having to pass it down to them<br/>
/// <see cref="GetException"/> will be evaluated lazily when <see cref="IExceptionBuilder.GetException"/> of the parent is called<br/>
/// <seealso cref="ExceptionBuilder.AddChild"/>
/// </summary>
public interface IExceptionProvider
{
    /// <summary>
    /// evaluates the exception lazily and returns if applicable.
    /// </summary>
    /// <returns>null, if no error occured, </returns>
    Exception? GetException();
}