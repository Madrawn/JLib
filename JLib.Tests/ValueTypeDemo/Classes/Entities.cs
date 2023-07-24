namespace JLib.Tests.ValueTypeDemo.Classes;

public interface IEntity { }
public abstract class CommandEntity : IEntity
{
}
public abstract class QueryEntity : IEntity
{
}
public interface IReadOnlyEntity : IEntity
{
}
