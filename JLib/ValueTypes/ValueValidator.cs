using System.ComponentModel.DataAnnotations;
using JLib.Exceptions;

namespace JLib.ValueTypes;

public abstract class ValueValidator<TValue> : IExceptionProvider
{
    private readonly string _valueTypeName;
    protected TValue Value { get; }
    protected readonly List<string> Messages = new();
    protected ValueValidator(TValue value, string valueTypeName)
    {
        _valueTypeName = valueTypeName;
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
            $"{_valueTypeName} validation failed: '{Value}' is not a valid Value.",
            Messages.Distinct().Select(msg => new ValidationException(msg)));
}