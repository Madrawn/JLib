using System.ComponentModel.DataAnnotations;
using JLib.Exceptions;
using JLib.Helper;
using Serilog;
using Serilog.Events;

namespace JLib.ValueTypes;

public abstract class ValueValidator<TValue> : IExceptionProvider
{
    protected string ValueTypeName { get; }
    protected TValue Value { get; }
    private readonly List<string> _messages = new();
    public IReadOnlyCollection<string> Messages => _messages;

    protected ValueValidator(TValue value, string valueTypeName)
    {
        ValueTypeName = valueTypeName;
        Value = value;
    }

    public void AddError(string message, string? hint = null)
    {
        if (hint != null)
            message += $" this might be resolved by {hint}";
        _messages.Add(message);
    }


    Exception? IExceptionProvider.GetException()
        => BuildException(_messages);

    public void ThrowIfNotEmpty(LogEventLevel? level = null)
    {
        var ex = this.CastTo<IExceptionProvider>().GetException();
        if (level is not null && ex is not null)
            Log.Write(level.Value, ex, "an aggregate exception has been thrown");
        if (ex is not null)
            throw ex;
    }

    protected virtual Exception? BuildException(IReadOnlyCollection<string> messages)
        => JLibAggregateException.ReturnIfNotEmpty(
            $"{ValueTypeName} validation failed: '{Value}' is not a valid Value.",
            _messages.Distinct().Select(msg => new ValidationException(msg)));
}