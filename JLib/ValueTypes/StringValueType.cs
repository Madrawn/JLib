using JLib.Exceptions;
using JLib.Helper;

namespace JLib.ValueTypes;

public abstract record StringValueType(string Value) : ValueType<string>(Value)
{
    protected StringValueType(string value, Action<StringValidator>? validate) : this(value)
    {
        var validator = new StringValidator(Value, GetType().FullClassName());
        validate?.Invoke(validator);
        validator.CastTo<IExceptionProvider>().GetException()?.Throw();
    }
}
public class StringValidator : ValueValidator<string?>
{
    public StringValidator(string? value, string valueTypeName) : base(value, valueTypeName) { }

    public StringValidator NotBeNull()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Value is null)
            AddError("Value is null");
        return this;
    }
    public StringValidator NotBeNullOrEmpty()
    {
        if (Value.IsNullOrEmpty())
            AddError("Value is null or empty");
        return this;
    }

    public StringValidator BeOneOf(IReadOnlyCollection<string> validValues)
    {
        if (!validValues.Contains(Value))
            AddError("Value is not one of the following: " + string.Join(", ", validValues));
        return this;
    }
    public StringValidator BeAlphanumeric()
        => SatisfyCondition(char.IsLetterOrDigit, nameof(BeAlphanumeric));
    public StringValidator SatisfyCondition(Func<char, bool> validator, string name)
    {
        if (Value is null)
        {
            AddError(name + "failed: string is null");
            return this;
        }
        var errorIndices = Value
            .AddIndex()
            .Where(x => !validator(x.Item1))
            .Select(x => x.Item2)
            .ToArray();
        if (errorIndices.Any())
            AddError($"{name} failed at index [{string.Join(", ", errorIndices)}]");

        if (Value.All(validator))
            return this;

        AddError(name + " failed");
        return this;
    }
    public StringValidator NotBeNullOrWhitespace()
    {
        if (Value.IsNullOrEmpty())
            AddError("Value is null or whitespace");
        return this;
    }
    public StringValidator StartWith(string prefix)
    {
        NotBeNull();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Value == null || !Value.StartsWith(prefix))
            AddError($"Value does not start with {prefix}");
        return this;
    }
    public StringValidator NotContain(string value)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Value != null && Value.Contains(value))
            AddError($"Value does not contain {value}");
        return this;
    }

    /// <summary>
    /// expects the string to be
    /// <list type="bullet">
    ///     <item><see cref="NotBeNullOrWhitespace"/></item>
    ///     <item><see cref="NotContainWhitespace"/></item>
    ///     <item><see cref="NotContain"/>("&lt;")</item>
    ///     <item><see cref="NotContain"/>("&gt;")</item>
    ///     <item><see cref="StartWith"/>("https://")</item>
    ///     <item>be creatable with Uri.TryCreate(Value, UriCreationOptions.Absolute,out result)</item>
    ///     <item>and return result.Scheme == <see cref="Uri.UriSchemeHttps"/></item>
    /// </list>
    /// </summary>
    public StringValidator BeHttpsUrl()
    {
        NotBeNullOrWhitespace();
        NotContainWhitespace();
        NotContain("<");
        NotContain(">");
        StartWith("https://");
        if (!Uri.TryCreate(Value, UriKind.Absolute, out var uriResult)
            || uriResult.Scheme != Uri.UriSchemeHttps)
            AddError("Value is not a valid https uri");

        return this;
    }

    /// <summary>
    /// expects <see cref="char.IsAscii(char)"/> to be true for all characters of the string.<br/>
    /// <inheritdoc cref="char.IsAscii(char)"/>
    /// </summary>
    public StringValidator BeAscii()
        => SatisfyCondition(char.IsAscii, nameof(BeAscii));

    /// <summary>
    /// expects <see cref="char.IsLetterOrDigit(char)"/> to be true for all characters of the string.<br/>
    /// <inheritdoc cref="char.IsLetterOrDigit(char)"/>
    /// </summary>
    public StringValidator OnlyContainAlphaNumericCharacters()
        => SatisfyCondition(char.IsLetterOrDigit, nameof(OnlyContainAlphaNumericCharacters));

    /// <summary>
    /// expects <see cref="char.IsWhiteSpace(char)"/> to be false for all characters of the string.<br/>
    /// <inheritdoc cref="char.IsWhiteSpace(char)"/>
    /// </summary>
    public StringValidator NotContainWhitespace()
        => Value is null ? this : SatisfyCondition(c => !char.IsWhiteSpace(c), nameof(NotContainWhitespace));

    /// <summary>
    /// expects <see cref="char.IsNumber(char)"/> to be true for all characters of the string.<br/>
    /// <inheritdoc cref="char.IsNumber(char)"/>
    /// </summary>
    public StringValidator BeNumeric()
        => SatisfyCondition(char.IsNumber, nameof(BeNumeric));
    public StringValidator MinimumLength(int length)
    {
        NotBeNull();
        if (Value?.Length < length)
            AddError($"the value has to be at least {length} characters long but got length {Value.Length}");
        return this;
    }
    public StringValidator MaximumLength(int length)
    {
        NotBeNull();
        if (Value?.Length > length)
            AddError($"the value has to be at most {length} characters long but got length {Value.Length}");
        return this;
    }
    public StringValidator BeOfLength(int length)
    {
        NotBeNull();
        if (Value?.Length != length)
            AddError($"the value has to be exactly {length} characters long but got length {Value?.Length}");
        return this;
    }

    public StringValidator EndWith(string value)
    {
        if (Value?.EndsWith(value) != true)
            AddError($"the value has to end with '{value}'");
        return this;
    }
}