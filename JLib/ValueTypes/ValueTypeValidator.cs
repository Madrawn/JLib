using System.ComponentModel.DataAnnotations;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.ValueTypes;

public abstract class ValueTypeValidator<TValue> : IExceptionProvider
{
    protected TValue Value { get; }
    protected readonly List<string> Messages = new();
    protected ValueTypeValidator(TValue value)
    {
        Value = value;
    }
    public void AddError(string message, string? hint = null)
    {
        if (hint != null)
            message += $" this might be resolved by {hint}";
        Messages.Add(message);
    }

    Exception? IExceptionProvider.GetException()
        => BuildException(Messages);
    
    protected virtual Exception? BuildException(IReadOnlyCollection<string> messages)
        => JLibAggregateException.ReturnIfNotEmpty(
            $"{typeof(TValue).FullClassName()} validation failed: {Value} is not a valid Value.",
            Messages.Distinct().Select(msg => new ValidationException(msg)));
}