namespace JLib.ValueTypes;

public abstract record IntValueType(int Value) : ValueType<int>(Value)
{
    protected IntValueType(int Value, Action<IntValidator> validator) : this(Value)
    {
        var val = new IntValidator(Value, GetType().Name);
        validator(val);
    }
}