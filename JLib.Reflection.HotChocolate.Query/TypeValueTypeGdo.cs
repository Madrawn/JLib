namespace JLib.Reflection.HotChocolate.Query;
public class TypeValueTypeGdo
{
    private readonly TypeValueType _src;
    public TypeGdo Self => new(_src.GetType());
    public TypeGdo Value => new(_src.Value);
    public TypeValueTypeGdo(TypeValueType src)
    {
        _src = src;
    }
}