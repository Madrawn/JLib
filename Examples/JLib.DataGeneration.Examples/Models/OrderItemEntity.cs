using JLib.ValueTypes;

namespace JLib.DataGeneration.Examples.Models;

public record OrderItemId(Guid Value) : GuidValueType(Value);

public class OrderItemEntity
{
    public OrderItemEntity(OrderId orderId, ArticleId articleId, int quantity, double price)
    {
        Id = new(Guid.NewGuid());
        OrderId = orderId;
        ArticleId = articleId;
        Quantity = quantity;
        Price = price;
    }
    public OrderItemId Id { get; init; }
    public OrderId OrderId { get; init; }
    public ArticleId ArticleId { get; init; }
    public int Quantity { get; init; }
    public double Price { get; init; }
}