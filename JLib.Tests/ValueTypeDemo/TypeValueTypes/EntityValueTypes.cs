using JLib.Tests.ValueTypeDemo.BaseTypes;

using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib.Tests.ValueTypeDemo.TypeValueTypes;
public partial class TypeValueTypes
{
    [Implements<IEntity>]
    public abstract record Entity(Type Value) : TypeValueType(Value);


    [IsAssignableTo<BaseTypes.QueryEntity>, IsClass, NotAbstract]
    public record QueryEntity(Type Value) : Entity(Value);

    [IsAssignableTo<BaseTypes.CommandEntity>, IsClass, NotAbstract]
    public record CommandEntity(Type Value) : Entity(Value);
    [IsAssignableTo<IReadOnlyEntity>, IsInterface]
    public record ReadOnlyEntity(Type Value) : Entity(Value);
}
