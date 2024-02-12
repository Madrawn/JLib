namespace JLib.ValueTypes;

public abstract record GuidValueType(Guid Value) : ValueType<Guid>(Value);