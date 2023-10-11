using JLib.Data;

namespace JLib.HotChocolate;
public interface IGraphQlDataObject : IDataObject
{

}
public interface IMappedGraphQlDataObject<TEntity> : IGraphQlDataObject
    where TEntity : IEntity
{ }