namespace JLib.DataProvider;

public interface ISourceDataProviderRw<TData> : IDataProviderRw<TData>, ISourceDataProviderR<TData>
    where TData : IEntity
{
}