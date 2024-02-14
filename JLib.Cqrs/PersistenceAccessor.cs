namespace JLib.Cqrs;

public interface IPersistenceAccessor
{
    public void SaveChanges();
}