namespace JLib;

public abstract record StringValueType(string Value) : ValueType<string>(Value);

public abstract record GuidValueType(Guid Value) : ValueType<Guid>(Value);
