using System.Collections.Concurrent;
using JLib.Helper;
using Serilog;
using Serilog.Events;

namespace JLib.Exceptions;

public interface IExceptionProvider
{
    Exception? GetException();
}

public interface IExceptionManager : IExceptionProvider
{
    /// <summary>
    /// wraps the exception in a try catch block and adds the exception on throw.
    /// </summary>
    /// <param name="action"></param>
    void TryExecution(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            Add(e);
        }
    }
    /// <summary>
    /// wraps the exception in a try catch block and adds the exception on throw.
    /// </summary>
    void TryExecution(string message, Action<IExceptionManager> action)
    {
        try
        {
            action(CreateChild(message));
        }
        catch (Exception e)
        {
            Add(e);
        }
    }

    void Add(Exception exception);
    void Add(IEnumerable<Exception> exceptions);
    void ThrowIfNotEmpty(LogEventLevel? level = null);
    IExceptionManager CreateChild(string message);
    void CreateChild(string message, IEnumerable<Exception> childExceptions);
    void AddChild(IExceptionProvider exceptionProvider);
}
public class ExceptionManager : IExceptionManager
{
    private readonly string _message;
    private readonly ConcurrentBag<Exception> _exceptions = new();
    private readonly ConcurrentBag<IExceptionProvider> _children = new();

    private IEnumerable<Exception?> BuildExceptionList()
        => _exceptions.Concat(_children.Select(c => c.GetException()));
    public void Add(Exception exception) => _exceptions.Add(exception);

    public void Add(IEnumerable<Exception> exceptions)
    {
        foreach (var exception in exceptions)
            Add(exception);
    }

    public void ThrowIfNotEmpty(LogEventLevel? level = null)
    {
        var ex = JLibAggregateException.ReturnIfNotEmpty(_message, BuildExceptionList().WhereNotNull());
        if (level is not null && ex is not null)
            Log.Write(level.Value, ex, "an aggregate exception has been thrown");
        if (ex is not null)
            throw ex;
    }

    public Exception? GetException() => JLibAggregateException.ReturnIfNotEmpty(_message, BuildExceptionList());

    public ExceptionManager(string message)
    {
        _message = message;
    }

    public IExceptionManager CreateChild(string message)
    {
        var child = new ExceptionManager(message);
        _children.Add(child);
        return child;
    }

    public void CreateChild(string message, IEnumerable<Exception> childExceptions)
    {
        var exceptions = childExceptions.ToArray();
        if (exceptions.None())
            return;
        var child = CreateChild(message);
        child.Add(exceptions);
    }

    public void AddChild(IExceptionProvider exceptionProvider)
        => _children.Add(exceptionProvider);
}