using System.ComponentModel.DataAnnotations;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.ValueTypes;

/// <summary>
/// represents a class which validates a value of type <typeparamref name="TValue"/><br/>
/// requires a constructor with the signature <code>public <see cref="ValidationContext{TValue}"/>(<typeparamref name="TValue"/> value, <see cref="string"/> valueTypeName)</code>
/// </summary>
/// <typeparam name="TValue">the type of the <see cref="Value"/> to be validated</typeparam>
public interface IValidationContext<out TValue> : IExceptionProvider
{
    /// <summary>
    /// the type this value will be assigned to
    /// </summary>
    public Type TargetType { get; }
    /// <summary>
    /// the value which is being validated
    /// </summary>
    public TValue Value { get; }
    /// <summary>
    /// all currently added messages
    /// </summary>
    public IReadOnlyCollection<string> Messages { get; }
    /// <summary>
    /// adds the <paramref name="message"/> and <paramref name="hint"/> to the <see cref="Messages"/>
    /// </summary>
    /// <param name="message">the message to be added</param>
    /// <param name="hint"></param>
    void AddError(string message, string? hint = null);
    /// <summary>
    /// Adds the given <paramref name="subProvider"/> as a validator to this one.<br/>
    /// This might be used to validate complex types (like <see cref="Type"/>)
    /// </summary>
    /// <param name="subProvider"></param>
    void AddSubValidators(IExceptionProvider subProvider);

}

/// <summary>
/// <see cref="IExceptionProvider"/> for validating values of type <typeparamref name="TValue"/><br/>
/// often used to validate <see cref="ValueType{TValue}"/>s
/// </summary>
public class ValidationContext<TValue> : IValidationContext<TValue>
{
    /// <summary>
    /// <inheritdoc cref="IValidationContext{TValue}.TargetType"/>
    /// </summary>
    public Type TargetType { get; }
    /// <summary>
    /// the value which is currently being validated
    /// </summary>
    public TValue Value { get; }
    private readonly List<string> _messages = new();
    private readonly List<IExceptionProvider> _subValidators = new();
    /// <summary>
    /// a list of all validation errors
    /// </summary>
    public IReadOnlyCollection<string> Messages => _messages;

    public ValidationContext(TValue value, Type targetType)
    {
        TargetType = targetType;
        Value = value;
    }

    /// <summary>
    /// adds an <see cref="Exception"/> with the given <paramref name="message"/> as <see cref="Exception.Message"/> to the validation result
    /// </summary>
    /// <param name="message">the <see cref="Exception.Message"/></param>
    /// <param name="hint">a hint appended to the <see cref="Exception.Message"/> which might help the user to resolve this issue</param>
    public void AddError(string message, string? hint = null)
    {
        if (hint != null)
            message += $" this might be resolved by {hint}";
        _messages.Add(message);
    }

    /// <summary>
    /// adds the <paramref name="subProvider"/> as a sub validator to this validator
    /// </summary>
    /// <param name="subProvider"></param>
    public void AddSubValidators(IExceptionProvider subProvider)
        => _subValidators.Add(subProvider);

    Exception? IExceptionProvider.GetException()
        => BuildException(_messages, _subValidators);

    // todo: lazy validation (do not run the entire validation if we just need to check if anything failed, instead abort validation once one validator failed)
    /// <summary>
    /// <inheritdoc cref="IExceptionProvider.HasErrors"/>
    /// </summary>
    public bool HasErrors() => _messages.Any() || _subValidators.Any(v => v.HasErrors());

    /// <summary>
    /// <inheritdoc cref="IExceptionProvider.GetException"/>
    /// </summary>
     protected virtual Exception? BuildException(IReadOnlyCollection<string> messages, IReadOnlyCollection<IExceptionProvider> provider)
        => JLibAggregateException.ReturnIfNotEmpty(
            $"{TargetType.FullName()} validation failed: '{Value}' is set up incorrectly.",
            provider
                .Select(p => p.GetException())
                .Prepend(
                    JLibAggregateException.ReturnIfNotEmpty(
                        "Value Validation Failed",
                        _messages.Distinct().Select(msg => new ValidationException(msg))
                    )
                )
        );
}