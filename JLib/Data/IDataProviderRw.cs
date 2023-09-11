using JLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib.Data;

public interface IEntity
{
}

public interface IGraphQlMutationParameter
{

}
public interface IGraphQlMutationParameter<TEntity> : IGraphQlMutationParameter
{

}
public interface IGraphQlDataObject
{

}
public interface IGraphQlDataObject<TEntity> : IGraphQlDataObject
    where TEntity : IEntity
{
    
}


public interface IDataProviderR<TData>
{
    public IQueryable<TData> Get();

}
public interface IDataProviderRw<TData> : IDataProviderR<TData>
    where TData : IEntity
{
    public void Add(TData item);
    public void Add(IEnumerable<TData> items);
    public void Remove(Guid item);
    public void Remove(IEnumerable<Guid> items);
}
