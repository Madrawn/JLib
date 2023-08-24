namespace JLib;

public abstract record ValueType<T>(T Value)
{

    public virtual void Deconstruct(out T value)
    {
        value = this.Value;
    }
}
