using JLib.DataProvider;
using JLib.ValueTypes;
using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.Cqrs;

/// <summary>
/// enables the <see cref="QueryableHelper.ById{T,TId}"/> extention method
/// </summary>
public interface ITypedIdDataObject<TId> : IDataObject
    where TId : GuidValueType
{
}

/// <summary>
/// marks an entity as being the primary domain representation using value types etc for the command side of the CQRS app.
/// </summary>
public interface ICommandEntity : IEntity
{
}

/// <summary>
/// An entity which uses ValueTypes to ensure data validity
/// </summary>
[Implements(typeof(ICommandEntity)), IsClass, NotAbstract, Priority(EntityType.NextPriority)]
public record CommandEntityType(Type Value) : EntityType(Value)
{
    public new const int NextPriority = EntityType.NextPriority - 1_000;
}