using System.Runtime.CompilerServices;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.ValueTypes;

public abstract record StringValueType(string Value) : ValueType<string>(Value)
{
    protected StringValueType(string value, Action<StringValueTypeValidator> validate) : this(value)
    {
        var validator = new StringValueTypeValidator(this);
        validate(validator);
        validator.CastTo<IExceptionProvider>().GetException()?.Throw();
    }
}
public class StringValueTypeValidator : ValueTypeValidator<StringValueType, string>
{
    public StringValueTypeValidator(StringValueType valueType) : base(valueType) { }

    public StringValueTypeValidator NotBeNull()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Value is null)
            AddError("Value is null");
        return this;
    }
    public StringValueTypeValidator NotBeNullOrEmpty()
    {
        if (Value.IsNullOrEmpty())
            AddError("Value is null or empty");
        return this;
    }
    public StringValueTypeValidator NotBeNullOrWhitespace()
    {
        if (Value.IsNullOrEmpty())
            AddError("Value is null or whitespace");
        return this;
    }
    /// <summary>
    /// Checks whether any character in the string satisfies <see cref="char.IsWhiteSpace(char)"/><br/>
    /// </summary>
    /// <returns></returns>
    public StringValueTypeValidator ContainNoWhitespace()
    {
        var whitespaceIndex = Value
            .AddIndex()
            .Where(x => char.IsWhiteSpace(x.Item1))
            .Select(x => x.Item2)
            .ToArray();
        if (whitespaceIndex.Any())
            AddError($"Value contains a whitespace at {string.Join(", ", whitespaceIndex)}");
        return this;
    }
}