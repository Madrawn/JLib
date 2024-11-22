namespace JLib.ValueTypes;


/// <summary>
/// <inheritdoc cref="NumericValueType{T}"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public interface INumericValueType<out T>
    where T : struct
#if NET7_0_OR_GREATER
    , System.Numerics.INumber<T>
#endif
{
    /// <summary>
    /// <inheritdoc cref="ValueType{T}.Value"/>
    /// </summary>
    T Value { get; }
}

/// <summary>
/// Represents a numeric value type.
/// Enforces the <typeparamref name="T"/> <see cref="System.Numerics.INumber{TSelf}"/> when using .net7 or greater.
/// There are specific implementations for each numeric type.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="SByteValueType"/>
/// <seealso cref="ByteValueType"/>
/// <seealso cref="ShortValueType"/>
/// <seealso cref="UShortValueType"/>
/// <seealso cref="IntValueType"/>
/// <seealso cref="UIntValueType"/>
/// <seealso cref="LongValueType"/>
/// <seealso cref="ULongValueType"/>
/// <seealso cref="FloatValueType"/>
/// <seealso cref="DoubleValueType"/>
/// <seealso cref="DecimalValueType"/>
public abstract record NumericValueType<T> : ValueType<T>, INumericValueType<T>
    where T : struct
#if NET7_0_OR_GREATER
    , System.Numerics.INumber<T>
#endif
{
    /// <summary>
    /// <inheritdoc cref="NumericValueType{T}"/>
    /// </summary>
    /// <param name="Value"></param>
    private protected
    NumericValueType(T Value) : base(Value)
    {
    }
}

/// <summary>
/// Represents a value type for characters.
/// </summary>
/// <param name="Value">The character value.</param>
public abstract record CharValueType(char Value) : ValueType<char>(Value);

/// <summary>
/// Represents a value type for signed bytes.
/// </summary>
/// <param name="Value">The signed byte value.</param>
public abstract record SByteValueType(sbyte Value) : NumericValueType<sbyte>(Value);

/// <summary>
/// Represents a value type for bytes.
/// </summary>
/// <param name="Value">The byte value.</param>
public abstract record ByteValueType(byte Value) : NumericValueType<byte>(Value);

/// <summary>
/// Represents a value type for shorts.
/// </summary>
/// <param name="Value">The short value.</param>
public abstract record ShortValueType(short Value) : NumericValueType<short>(Value);

/// <summary>
/// Represents a value type for unsigned shorts.
/// </summary>
/// <param name="Value">The unsigned short value.</param>
public abstract record UShortValueType(ushort Value) : NumericValueType<ushort>(Value);

/// <summary>
/// Represents a value type for integers.
/// </summary>
/// <param name="Value">The integer value.</param>
public abstract record IntValueType(int Value) : NumericValueType<int>(Value);

/// <summary>
/// Represents a value type for unsigned integers.
/// </summary>
/// <param name="Value">The unsigned integer value.</param>
public abstract record UIntValueType(uint Value) : NumericValueType<uint>(Value);

/// <summary>
/// Represents a value type for longs.
/// </summary>
/// <param name="Value">The long value.</param>
public abstract record LongValueType(long Value) : NumericValueType<long>(Value);

/// <summary>
/// Represents a value type for unsigned longs.
/// </summary>
/// <param name="Value">The unsigned long value.</param>
public abstract record ULongValueType(ulong Value) : NumericValueType<ulong>(Value);

/// <summary>
/// Represents a value type for floats.
/// </summary>
/// <param name="Value">The float value.</param>
public abstract record FloatValueType(float Value) : NumericValueType<float>(Value);

/// <summary>
/// Represents a value type for doubles.
/// </summary>
/// <param name="Value">The double value.</param>
public abstract record DoubleValueType(double Value) : NumericValueType<double>(Value);

/// <summary>
/// Represents a value type for decimals.
/// </summary>
/// <param name="Value">The decimal value.</param>
public abstract record DecimalValueType(decimal Value) : NumericValueType<decimal>(Value);

/// <summary>
/// Represents a value type for GUIDs.
/// </summary>
/// <param name="Value">The GUID value.</param>
public abstract record GuidValueType(Guid Value) : ValueType<Guid>(Value);

/// <summary>
/// Represents a base class for string value types.
/// Contains a <see cref="StringValidationContextExtensions.NotBeNull"/> Validation.
/// </summary>
/// <param name="Value">The string value.</param>
public abstract record StringValueType(string Value) : ValueType<string>(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string?> v)
        => v.NotBeNull();
}