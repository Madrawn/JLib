namespace JLib;

public abstract record StringValueType(string Value) : ValueType<string>(Value);

public abstract record GuidValueType(Guid Value) : ValueType<Guid>(Value);


public record Prefix(string Value) : StringValueType(Value);

public record PropertyPrefix(string Value) : Prefix(Value)
{
    public static implicit operator PropertyPrefix?(string? value)
        => value is null ? null : new(value);

}
public record ClassPrefix(string Value) : Prefix(Value);