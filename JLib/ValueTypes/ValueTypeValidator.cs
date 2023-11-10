using System.ComponentModel.DataAnnotations;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.ValueTypes;

public abstract class ValueTypeValidator<TValueType, TValue> : IExceptionProvider
    where TValueType : ValueType<TValue>
{
    protected readonly TValueType ValueType;
    protected TValue Value => ValueType.Value;
    protected readonly List<string> Messages = new();
    protected ValueTypeValidator(TValueType valueType)
    {
        ValueType = valueType;
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
            Messages.Select(msg => new ValidationException(msg)));
}