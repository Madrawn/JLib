using System.Collections.Concurrent;
using JLib.Helper;

namespace JLib.Exceptions;

/// <summary>
/// Implementation of the <see cref="IExceptionBuilder"/><br/>
/// <inheritdoc cref="IExceptionBuilder"/>
/// </summary>
public class ExceptionBuilder : IExceptionBuilder
{
    private readonly string _message;
    private readonly ConcurrentBag<Exception> _exceptions = new();
    private readonly ConcurrentBag<IExceptionProvider> _children = new();

    private IEnumerable<Exception?> BuildExceptionList()
        => _exceptions.Concat(_children.Select(c => c.GetException()));

    public virtual void Add(Exception exception) => _exceptions.Add(exception);
    public virtual void Add(IEnumerable<Exception> exceptions)
    {
        foreach (var exception in exceptions)
            Add(exception);
    }

    public virtual Exception? GetException() => JLibAggregateException.ReturnIfNotEmpty(_message, BuildExceptionList().WhereNotNull());

    public static IExceptionBuilder Create(string message)
        => new ExceptionBuilder(message);
    protected ExceptionBuilder(string message) => _message = message;

    public virtual IExceptionBuilder CreateChild(string message)
    {
        var child = ExceptionBuilder.Create(message);
        _children.Add(child);
        return child;
    }
    public virtual void CreateChild(string message, IEnumerable<Exception> childExceptions)
    {
        var exceptions = childExceptions.ToArray();
        if (exceptions.None())
            return;
        var child = CreateChild(message);
        child.Add(exceptions);
    }

    public virtual void AddChild(IExceptionProvider exceptionProvider)
        => _children.Add(exceptionProvider);
}