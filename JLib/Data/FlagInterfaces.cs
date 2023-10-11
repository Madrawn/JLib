namespace JLib.Data;

public interface IDataObject
{
    public Guid Id { get; }

}
/// <summary>
/// enables a class to be requested and edited via <see cref="IDataProviderRw{TData}"/> 
/// </summary>
public interface IEntity : IDataObject
{
}
/// <summary>
/// marks an entity as being the primary domain representation using value types etc for the command side of the CQRS app.
/// </summary>
public interface ICommandEntity : IEntity
{

}
/// <summary>
/// flag for the <see cref="MappedCommandEntityType"/>.
/// <br/>generates a 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IMappedCommandEntity<TEntity> : ICommandEntity
    where TEntity : IEntity
{ }