namespace JLib.ValueTypes;

public abstract record ULongValueType(ulong Value) : ValueType<ulong>(Value);