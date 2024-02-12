namespace JLib.DataProvider;

public sealed class IgnoredDataObject : IDataObject
{
    private IgnoredDataObject()
    {
    }

    public Guid Id { get; }
}