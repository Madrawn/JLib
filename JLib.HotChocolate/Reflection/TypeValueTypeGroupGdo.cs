namespace JLib.HotChocolate.Reflection;

public class TypeValueTypeGroupGdo
{
    public TypeGdo TypeValueTypeType { get; }
    [UseFiltering]
    public TypeGdo[] Types { get; }
    public TypeValueTypeGroupGdo(TypeGdo tvtt, TypeGdo[] types)
    {
        TypeValueTypeType = tvtt;
        Types = types;
    }
}