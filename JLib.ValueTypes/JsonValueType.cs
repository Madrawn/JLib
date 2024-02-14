namespace JLib.ValueTypes;

public record JsonStringValueType(string Value) : StringValueType(Value, v => v.NotBeNullOrWhitespace());