namespace JLib.ValueTypes;

public abstract record ValueType<T>(T Value)
{

    public virtual void Deconstruct(out T value)
    {
        value = Value;
    }
}
