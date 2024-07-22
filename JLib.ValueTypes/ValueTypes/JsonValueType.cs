namespace JLib.ValueTypes;

/// <summary>
/// Indicates, that the value should be a json string. This is not confirmed, as it would lead to performance issues.<br/>
/// validates for not being null or whitespace
/// </summary>
/// <param name="Value"></param>
public record JsonStringValueType(string Value)
    : StringValueType(Value)
{
    [Validation]
    private void Validator(ValidationContext<string?> v)
        => v.NotBeNullOrWhitespace();
}