using JLib.ValueTypes;

namespace JLib.DataGeneration.Examples.Models;

public record ArticleId(Guid Value) : GuidValueType(Value);

public class ArticleEntity
{
    public ArticleEntity(string name, double price)
    {
        Id = new(Guid.NewGuid());
        Name = name;
        Price = price;
    }
    public ArticleId Id { get; init; }
    public string Name { get; init; }
    public double Price { get; init; }
}