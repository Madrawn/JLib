using JLib.Exceptions;

namespace JLib;

public class TvtValidator : IExceptionProvider
{
    private readonly TypeValueType _typeValueType;
    private Type Value => _typeValueType.Value;
    private readonly List<string> _messages = new();
    public TvtValidator(TypeValueType typeValueType)
    {
        _typeValueType = typeValueType;
    }
    public void Add(string message)
        => _messages.Add(message);

    Exception? IExceptionProvider.GetException()
        => JLibAggregateException.ReturnIfNotEmpty(
            "Type validation failed",
            _messages.Select(msg => new InvalidTypeException(_typeValueType.GetType(), _typeValueType.Value, msg)));

    public void MustBeGeneric()
    {
        if (!Value.IsGenericType)
            Add("Must be Generic");
    }
    public void MustHaveNTypeArguments(int argumentCount)
    {
        MustBeGeneric();

        if (!Value.IsGenericType)
            Add("Must be Generic");
        if (Value.GenericTypeArguments.Length != argumentCount)
            Add($"It must have exactly {argumentCount} type arguments but got {Value.GenericTypeArguments.Length}");

    }

}