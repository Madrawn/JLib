namespace JLib.DataProvider;


/// <summary>
/// can be used as type argument if the given argument has to implement <see cref="IEntity"/> but is ignored due to reflection.
/// </summary>
public sealed class IgnoredEntity : IEntity
{
    private IgnoredEntity()
    {
    }

    /// <summary>
    /// <inheritdoc cref="IDataObject.Id"/>
    /// </summary>
    public Guid Id => default;
}
/// <summary>
/// can be used as type argument if the given argument has to implement <see cref="IDataObject"/> but is ignored due to reflection.
/// </summary>
public sealed class IgnoredDataObject : IDataObject
{
    private IgnoredDataObject()
    {
    }

    /// <summary>
    /// <inheritdoc cref="IDataObject.Id"/>
    /// </summary>
    public Guid Id => default;
}