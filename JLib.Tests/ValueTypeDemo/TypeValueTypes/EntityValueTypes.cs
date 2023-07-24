using JLib.Tests.ValueTypeDemo.Classes;

using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib.Tests.ValueTypeDemo.TypeValueTypes;
public partial class TypeValueTypes
{
    [Implements<IEntity>]
    public abstract record Entity(Type Value) : TypeValueType(Value);


    [DerivesFrom<Classes.QueryEntity>, IsClass, NotAbstract]
    public record QueryEntity(Type Value) : Entity(Value);

    [DerivesFrom<Classes.CommandEntity>, IsClass, NotAbstract]
    public record CommandEntity(Type Value) : Entity(Value);
    [DerivesFrom<IReadOnlyEntity>, IsInterface]
    public record ReadOnlyEntity(Type Value) : Entity(Value);
}
