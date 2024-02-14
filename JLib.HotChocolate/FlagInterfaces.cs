using JLib.DataProvider;

namespace JLib.HotChocolate;

public sealed class IgnoredGdo : IGraphQlDataObject
{
    private IgnoredGdo() { }
    public Guid Id { get; }
}

public interface IQueryDataObject : IDataObject
{

}
public interface IGraphQlDataObject : IQueryDataObject
{

}