namespace JLib;

public abstract record StringValueType(string Value) : ValueType<string>(Value);