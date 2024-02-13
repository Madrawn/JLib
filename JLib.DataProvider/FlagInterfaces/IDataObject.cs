namespace JLib.DataProvider;

/// <summary>
/// a class which contains Data.
/// <br/>might be an entity or a class which maps from an entity
/// </summary>
public interface IDataObject
{
    public Guid Id { get; }
}