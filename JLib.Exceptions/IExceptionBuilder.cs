namespace JLib.Exceptions;

/// <summary>
/// Builds nested <see cref="JLibAggregateException"/>s
/// </summary>
public interface IExceptionBuilder : IExceptionProvider
{
    /// <summary>
    /// adds the given <paramref name="exception"/> to <see cref="JLibAggregateException.InnerExceptions"/>
    /// </summary>
    void Add(Exception exception);

    /// <summary>
    /// adds all <paramref name="exceptions"/> to <see cref="JLibAggregateException.InnerExceptions"/>
    /// </summary>
    void Add(IEnumerable<Exception> exceptions);

    /// <summary>
    /// creates a nested <see cref="IExceptionBuilder"/>
    /// </summary>
    /// <param name="message">written to <see cref="JLibAggregateException.UserMessage"/></param>
    /// <returns>a nested <see cref="IExceptionBuilder"/></returns>
    IExceptionBuilder CreateChild(string message);

    /// <summary>
    /// adds a nested <see cref="JLibAggregateException"/> with the given <paramref name="childExceptions"/> as <see cref="JLibAggregateException.InnerExceptions"/> if the <paramref name="childExceptions"/> are not empty
    /// </summary>
    /// <param name="message">written to <see cref="JLibAggregateException.UserMessage"/></param>
    /// <param name="childExceptions">written to <see cref="AggregateException.InnerExceptions"/></param>
    void CreateChild(string message, IEnumerable<Exception> childExceptions);

    /// <summary>
    /// adds the given <paramref name="exceptionProvider"/> as child<br/>
    /// <see cref="IExceptionProvider.GetException"/> of the <paramref name="exceptionProvider"/> is called when <see cref="IExceptionProvider.GetException"/> is called, thereby enabling adding this child before the related process has been completed.
    /// </summary>
    void AddChild(IExceptionProvider exceptionProvider);
}