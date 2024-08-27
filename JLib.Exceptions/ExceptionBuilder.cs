using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using JLib.Helper;

namespace JLib.Exceptions;

/// <summary>
/// Builds nested <see cref="JLibAggregateException"/>s
/// </summary>
public sealed class ExceptionBuilder : IExceptionProvider, IDisposable
{
    private readonly object _exceptionLock = new();
    private readonly object _childrenLock = new();
    private readonly string _message;
    private readonly List<Exception> _exceptions = new();
    private readonly List<IExceptionProvider> _children = new();
    private readonly ExceptionBuilder? _parent;
    private bool _disposed;

    private IReadOnlyCollection<Exception?> BuildExceptionList()
    {
        lock (_exceptionLock)
            lock (_childrenLock)
                return _exceptions
                    .Concat(_children.Select(c => c.GetException()))
                    .ToReadOnlyCollection();
    }
    /// <summary>
    /// adds the given <paramref name="message"/> as <see cref="Exception"/> to <see cref="AggregateException.InnerExceptions"/>
    /// </summary>
    /// <param name="message"></param>
    public void Add(string message)
        => Add(new Exception(message));

    /// <summary>
    /// adds the given <paramref name="exception"/> to <see cref="JLibAggregateException.InnerExceptions"/>
    /// </summary>
    public void Add(Exception exception)
    {
        lock (_exceptionLock)
        {
            CheckDisposed();
            _exceptions.Add(exception);
        }
    }

    /// <summary>
    /// adds all <paramref name="exceptions"/> to <see cref="JLibAggregateException.InnerExceptions"/>
    /// </summary>
    public void Add(IEnumerable<Exception> exceptions)
    {
        lock (_exceptionLock)
        {
            CheckDisposed();
            _exceptions.AddRange(exceptions);
        }
    }

    /// <summary>
    /// adds all <paramref name="exceptions"/> to <see cref="JLibAggregateException.InnerExceptions"/>
    /// </summary>
    public void Add(IEnumerable<string> exceptions)
    {
        Add(exceptions.Select(msg => new Exception(msg)));
    }

    /// <summary>
    /// <inheritdoc cref="IExceptionProvider.HasErrors"/>
    /// </summary>
    public bool HasErrors()
    {
        lock (_exceptionLock)
            lock (_childrenLock)
                return _exceptions.Count > 0 || _children.Any(c => c.HasErrors());
    }

    /// <summary>
    /// builds the exception based on the current builder state.
    /// </summary>
    /// <returns>null, if the builder has no exceptions</returns>
    public Exception? GetException()
        => JLibAggregateException.ReturnIfNotEmpty(_message, BuildExceptionList().WhereNotNull());

    /// <summary>
    /// <inheritdoc cref="ExceptionBuilder"/>
    /// </summary>
    /// <param name="message">written to <see cref="JLibAggregateException.UserMessage"/></param>
    public ExceptionBuilder(string message) : this(message, null) { }
    private ExceptionBuilder(string message, ExceptionBuilder? parent)
    {
        _message = message;
        _parent = parent;
    }

    /// <summary>
    /// creates a nested <see cref="ExceptionBuilder"/>
    /// </summary>
    /// <param name="message">written to <see cref="JLibAggregateException.UserMessage"/></param>
    /// <returns>a nested <see cref="ExceptionBuilder"/></returns>
    public ExceptionBuilder CreateChild(string message)
    {
        CheckDisposed();
        var child = new ExceptionBuilder(message, this);
        lock (_childrenLock)
            _children.Add(child);
        return child;
    }

    /// <summary>
    /// adds a nested <see cref="JLibAggregateException"/> with the given <paramref name="childExceptions"/> as <see cref="JLibAggregateException.InnerExceptions"/> if the <paramref name="childExceptions"/> are not empty
    /// </summary>
    /// <param name="message">written to <see cref="JLibAggregateException.UserMessage"/></param>
    /// <param name="childExceptions">written to <see cref="AggregateException.InnerExceptions"/></param>
    public void CreateChild(string message, IEnumerable<Exception> childExceptions)
    {
        CheckDisposed();
        var exceptions = childExceptions.ToReadOnlyCollection();
        if (exceptions.None())
            return;
        var child = CreateChild(message);
        child.Add(exceptions);
    }

    /// <summary>
    /// adds a nested <see cref="JLibAggregateException"/> with the given <paramref name="messages"/> as <see cref="JLibAggregateException.InnerExceptions"/> if the <paramref name="messages"/> are not empty
    /// </summary>
    /// <param name="message">written to <see cref="JLibAggregateException.UserMessage"/></param>
    /// <param name="messages">written to <see cref="AggregateException.InnerExceptions"/></param>
    public void CreateChild(string message, IEnumerable<string> messages)
    {
        CheckDisposed();
        var exceptions = messages.ToReadOnlyCollection();
        if (exceptions.None())
            return;
        var child = CreateChild(message);
        child.Add(exceptions.Select(msg => new Exception(msg)));
    }


    private void CheckDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(
                $"This {nameof(ExceptionBuilder)} has been disposed and can no longer be changed");
    }
    /// <summary>
    /// adds the given <paramref name="exceptionProvider"/> as child<br/>
    /// <see cref="IExceptionProvider.GetException"/> of the <paramref name="exceptionProvider"/> is called when <see cref="IExceptionProvider.GetException"/> is called, thereby enabling adding this child before the related process has been completed.
    /// </summary>
    public void AddChild(IExceptionProvider exceptionProvider)
    {
        CheckDisposed();
        lock (_childrenLock)
            _children.Add(exceptionProvider);
    }
    /// <summary>
    /// adds the given <paramref name="exceptionProviders"/> as child<br/>
    /// <see cref="IExceptionProvider.GetException"/> of the <paramref name="exceptionProviders"/> is called when <see cref="IExceptionProvider.GetException"/> is called, thereby enabling adding this child before the related process has been completed.
    /// </summary>
    public void AddChildren(IEnumerable<IExceptionProvider> exceptionProviders)
    {
        CheckDisposed();
        lock (_childrenLock)
            _children.AddRange(exceptionProviders);
    }

    /// <summary>
    /// if this <see cref="ExceptionBuilder"/> is a child and has no content, it is removed from the parent<br/>
    /// not disposing a <see cref="ExceptionBuilder"/> will NOT result in a Memory leak.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        if (_parent is not null && HasErrors() is false)
            _parent._children.Remove(this);
        else
            this.ThrowIfNotEmpty();
    }
    /// <summary>
    /// <inheritdoc cref="IExceptionProvider.ThrowIfNotEmpty"/>
    /// </summary>
    /// <param name="onThrow">Invoked before an Exception is thrown</param>
    // overload required to enable it being used without having to cast the instance to IExceptionProvider
    public void ThrowIfNotEmpty(Action<Exception>? onThrow = null)
    {
        var exception = GetException();

        if (exception is null)
            return;

        onThrow?.Invoke(exception);
        throw exception;
    }
}
