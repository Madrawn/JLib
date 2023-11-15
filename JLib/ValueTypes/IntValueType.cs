namespace JLib.ValueTypes;

public record IntValueType(int Value) : ValueType<int>(Value)
{
    protected IntValueType(int Value, Action<IntValidator> validator) : this(Value)
    {

    }
}

public class IntValidator : ValueValidator<int>
{
    public IntValidator(int value) : base(value)
    {
    }

    public IntValidator BeGreaterOrEqualThan(int minimum)
    {
        if (Value < minimum)
            AddError($"value is smaller than {minimum}");
        return this;
    }
    public IntValidator BeLessOrEqualThan(int maximum)
    {
        if (Value > maximum)
            AddError($"value is greater than {maximum}");
        return this;
    }
    public IntValidator BeGreaterThan(int minimum)
    {
        if (Value <= minimum)
            AddError($"value is smaller than or equal to {minimum}");
        return this;
    }
    public IntValidator BeLessThan(int maximum)
    {
        if (Value >= maximum)
            AddError($"value is greater than or equal to {maximum}");
        return this;
    }

    /// <summary>
    /// values >= 0 are valid
    /// </summary>
    public IntValidator NotBeNegative()
        => BeGreaterOrEqualThan(0);
    /// <summary>
    /// values > 0 are valid
    /// </summary>
    public IntValidator BePositive()
        => BeGreaterThan(0);

    /// <summary>
    /// only values between and including <paramref name="minimum"/> and <paramref name="maximum"/> are valid
    /// </summary>
    public IntValidator BeInBounds(int minimum, int maximum)
        => BeGreaterOrEqualThan(minimum)
            .BeLessOrEqualThan(maximum);
}