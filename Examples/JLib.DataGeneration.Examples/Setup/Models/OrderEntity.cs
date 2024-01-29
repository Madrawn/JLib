using JLib.ValueTypes;

namespace JLib.DataGeneration.Examples.Setup.Models;

public record OrderId(Guid Value) : GuidValueType(Value);

public class OrderEntity
{
    public OrderEntity(CustomerId customer, OrderStatus status)
    {
        Id = new(Guid.NewGuid());
        Customer = customer;
        Status = status;
    }

    public OrderId Id { get; init; }
    public CustomerId Customer { get; init; }
    public OrderStatus Status { get; init; }
}