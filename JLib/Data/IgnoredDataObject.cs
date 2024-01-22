namespace JLib.Data;

public sealed class IgnoredDataObject : IDataObject
{
    private IgnoredDataObject()
    {
    }

    public Guid Id { get; }
}