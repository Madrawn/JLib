namespace JLib.DataProvider;

public interface ISourceDataProviderR<TData> : IDataProviderR<TData>
    where TData : IDataObject
{
}