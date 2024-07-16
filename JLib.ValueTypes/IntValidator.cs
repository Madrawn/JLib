namespace JLib.ValueTypes;

/// <summary>
/// validates values of type <see cref="int"/>
/// </summary>
public class IntValidator : ValidationContext<int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IntValidator"/> class.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="targetType">The <see cref="Type"/> which <paramref name="value"/> is being validated for</param>
    public IntValidator(int value, Type targetType) : base(value, targetType)
    {
    }

    /// <summary>
    /// Validates that the value is greater than or equal to the specified minimum.
    /// </summary>
    /// <param name="minimum">The minimum value.</param>
    /// <returns>The current instance of the <see cref="IntValidator"/> class.</returns>
    public IntValidator BeGreaterOrEqualThan(int minimum)
    {
        if (Value < minimum)
            AddError($"value is smaller than {minimum}");
        return this;
    }

    /// <summary>
    /// Validates that the value is less than or equal to the specified maximum.
    /// </summary>
    /// <param name="maximum">The maximum value.</param>
    /// <returns>The current instance of the <see cref="IntValidator"/> class.</returns>
    public IntValidator BeLessOrEqualThan(int maximum)
    {
        if (Value > maximum)
            AddError($"value is greater than {maximum}");
        return this;
    }

    /// <summary>
    /// Validates that the value is greater than the specified minimum.
    /// </summary>
    /// <param name="minimum">The minimum value.</param>
    /// <returns>The current instance of the <see cref="IntValidator"/> class.</returns>
    public IntValidator BeGreaterThan(int minimum)
    {
        if (Value <= minimum)
            AddError($"value is smaller than or equal to {minimum}");
        return this;
    }

    /// <summary>
    /// Validates that the value is less than the specified maximum.
    /// </summary>
    /// <param name="maximum">The maximum value.</param>
    /// <returns>The current instance of the <see cref="IntValidator"/> class.</returns>
    public IntValidator BeLessThan(int maximum)
    {
        if (Value >= maximum)
            AddError($"value is greater than or equal to {maximum}");
        return this;
    }

    /// <summary>
    /// Validates that the value is not negative (greater than or equal to 0).
    /// </summary>
    /// <returns>The current instance of the <see cref="IntValidator"/> class.</returns>
    public IntValidator NotBeNegative()
        => BeGreaterOrEqualThan(0);

    /// <summary>
    /// Validates that the value is positive (greater than 0).
    /// </summary>
    /// <returns>The current instance of the <see cref="IntValidator"/> class.</returns>
    public IntValidator BePositive()
        => BeGreaterThan(0);

    /// <summary>
    /// Validates that the value is within the specified range (between and including the minimum and maximum).
    /// </summary>
    /// <param name="minimum">The minimum value.</param>
    /// <param name="maximum">The maximum value.</param>
    /// <returns>The current instance of the <see cref="IntValidator"/> class.</returns>
    public IntValidator BeInBounds(int minimum, int maximum)
        => BeGreaterOrEqualThan(minimum)
            .BeLessOrEqualThan(maximum);
}
