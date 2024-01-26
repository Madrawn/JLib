using JLib.AutoMapper;
using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;
using static JLib.Reflection.Attributes.TvtFactoryAttributes;

namespace JLib.Data;

/// <summary>
/// a class which contains Data.
/// <br/>might be an entity or a class which maps from an entity
/// </summary>
public interface IDataObject
{
    public Guid Id { get; }
}

public interface IDataObjectType : ITypeValueType
{
}

/// <summary>
/// the <see cref="MappedDataObjectProfile"/> will create a map for each <see cref="MappingInfo"/>
/// </summary>
public interface IMappedDataObjectType : IDataObjectType
{
    ExplicitTypeMappingInfo[] MappingInfo { get; }
}

public abstract record DataObjectType(Type Value) : NavigatingTypeValueType(Value), IDataObjectType
{
    public const int NextPriority = 10_000;
}

/// <summary>
/// enables a class to be requested and edited via <see cref="IDataProviderRw{TData}"/> 
/// </summary>
public interface IEntity : IDataObject
{
}

/// <summary>
/// enables the <see cref="QueryableHelper.ById{T,TId}"/> extention method
/// </summary>
public interface ITypedIdDataObject<TId> : IDataObject
    where TId : GuidValueType
{
}

/// <summary>
/// a class which directly accesses data using EfCore, a web api or other methods
/// </summary>
/// <param name="Value"></param>
[Implements(typeof(IEntity)), IsClass, NotAbstract]
public record EntityType(Type Value) : DataObjectType(Value), IValidatedType
{
    public new const int NextPriority = DataObjectType.NextPriority - 1_000;

    public virtual void Validate(ITypeCache cache, TvtValidator value)
    {
        if (GetType() == typeof(EntityType))
            value.AddError(
                $"You have to specify which type of entity this is by implementing a derivation of the {nameof(IEntity)} interface");
    }
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