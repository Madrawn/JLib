namespace JLib.ValueTypes;

/// <summary>
/// Represents a value type for characters.
/// </summary>
/// <param name="Value">The character value.</param>
public abstract record CharValueType(char Value) : ValueType<char>(Value);

/// <summary>
/// Represents a value type for signed bytes.
/// </summary>
/// <param name="Value">The signed byte value.</param>
public abstract record SByteValueType(sbyte Value) : ValueType<sbyte>(Value);

/// <summary>
/// Represents a value type for bytes.
/// </summary>
/// <param name="Value">The byte value.</param>
public abstract record ByteValueType(byte Value) : ValueType<byte>(Value);

/// <summary>
/// Represents a value type for shorts.
/// </summary>
/// <param name="Value">The short value.</param>
public abstract record ShortValueType(short Value) : ValueType<short>(Value);

/// <summary>
/// Represents a value type for unsigned shorts.
/// </summary>
/// <param name="Value">The unsigned short value.</param>
public abstract record UShortValueType(ushort Value) : ValueType<ushort>(Value);

/// <summary>
/// Represents a value type for integers.
/// </summary>
/// <param name="Value">The integer value.</param>
public abstract record IntValueType(int Value) : ValueType<int>(Value);

/// <summary>
/// Represents a value type for unsigned integers.
/// </summary>
/// <param name="Value">The unsigned integer value.</param>
public abstract record UIntValueType(uint Value) : ValueType<uint>(Value);

/// <summary>
/// Represents a value type for longs.
/// </summary>
/// <param name="Value">The long value.</param>
public abstract record LongValueType(long Value) : ValueType<long>(Value);

/// <summary>
/// Represents a value type for unsigned longs.
/// </summary>
/// <param name="Value">The unsigned long value.</param>
public abstract record ULongValueType(ulong Value) : ValueType<ulong>(Value);

/// <summary>
/// Represents a value type for floats.
/// </summary>
/// <param name="Value">The float value.</param>
public abstract record FloatValueType(float Value) : ValueType<float>(Value);

/// <summary>
/// Represents a value type for doubles.
/// </summary>
/// <param name="Value">The double value.</param>
public abstract record DoubleValueType(double Value) : ValueType<double>(Value);

/// <summary>
/// Represents a value type for decimals.
/// </summary>
/// <param name="Value">The decimal value.</param>
public abstract record DecimalValueType(decimal Value) : ValueType<decimal>(Value);

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