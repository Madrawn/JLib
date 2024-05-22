using System.ComponentModel.DataAnnotations;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.ValueTypes;

public interface IValidationProfile<in T>
{
    public IExceptionProvider Validate(T? value);
}

public abstract class ValidationProfile<TValue> : IValidationProfile<TValue>
{
    private readonly Type _owner;

    protected ValidationProfile(Type owner)
    {
        _owner = owner;
    }
    private List<Action<ExceptionBuilder, TValue?>> Validators { get; } = new();
    protected void AddValidator(Action<ExceptionBuilder, TValue?> validator)
        => Validators.Add(validator);
    public IExceptionProvider Validate(TValue? value)
    {
        var ex = new ExceptionBuilder(_owner.FullName(true));
        foreach (var validator in Validators)
            validator(ex, value);

        return ex;
    }
}

public class StringValidationProfile<TVt> : ValidationProfile<string>
    where TVt : ValueType<string>
{
    public StringValidationProfile(Action<StringValidator> validation) : base(typeof(TVt))
    {

    }
}

public record DemoVt : StringValueType
{
    [ValueTypeValidator]
    private static readonly IValidationProfile<string> Validator = new StringValidationProfile<DemoVt>(v => v.NotBeNull());

    public DemoVt(string Value) : base(Value)
    {
        ValueType.Validate<DemoVt, string>(Value).ThrowIfNotEmpty();
    }
}

/// <summary>
/// <see cref="IExceptionProvider"/> for validating values of type <typeparamref name="TValue"/><br/>
/// often used to validate <see cref="ValueType{TValue}"/>s
/// </summary>
public abstract class ValueValidator<TValue> : IExceptionProvider
{
    protected string ValueTypeName { get; }
    protected TValue Value { get; }
    private readonly List<string> _messages = new();
    private readonly List<IExceptionProvider> _subValidators = new();
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
    public void AddSubValidators(IExceptionProvider subProvider)
        => _subValidators.Add(subProvider);

    Exception? IExceptionProvider.GetException()
        => BuildException(_messages, _subValidators);

    /// <summary>
    /// <inheritdoc cref="IExceptionProvider.HasErrors"/>
    /// </summary>
    public bool HasErrors() => _messages.Any() || _subValidators.Any(v => v.HasErrors());

    /// <summary>
    /// <inheritdoc cref="IExceptionProvider.GetException"/>
    /// </summary>
    protected virtual Exception? BuildException(IReadOnlyCollection<string> messages, IReadOnlyCollection<IExceptionProvider> provider)
        => JLibAggregateException.ReturnIfNotEmpty(
            $"{ValueTypeName} validation failed: '{Value}' is not a valid Value.",
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