using JLib.Data;

namespace JLib.HotChocolate;

public sealed class IgnoredGdo : IGraphQlDataObject
{
    private IgnoredGdo() { }
    public Guid Id { get; }
}

public interface IGraphQlDataObject : IDataObject
{

}