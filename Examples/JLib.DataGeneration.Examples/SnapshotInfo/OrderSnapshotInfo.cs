using System.Diagnostics;
using System.Xml.Linq;
using JLib.DataGeneration.Examples.Setup.SystemUnderTest;
using JLib.DataGeneration.Examples.Setup.Models;

namespace JLib.DataGeneration.Examples.SnapshotInfo;

public class OrderSnapshotInfo
{
    public OrderSnapshotInfo(OrderEntity order, IIdRegistry idRegistry)
    {
        Id = order.Id.IdSnapshot(idRegistry);
        Customer = order.Customer.IdSnapshot(idRegistry);
        Status = order.Status;
    }
    public string Type => "Order";
    public IdSnapshotInformation Id { get; init; }
    public IdSnapshotInformation Customer { get; init; }
    public OrderStatus Status { get; init; }
}

public class ShoppingServiceSnapshotInfo
{
    public Dictionary<CustomerId, CustomerSnapshotInfo> Customers { get; }
    public Dictionary<OrderId, OrderSnapshotInfo> Orders { get; }
    public Dictionary<OrderItemId, OrderItemSnapshotInfo> OrderItems { get; }
    public Dictionary<ArticleId, ArticleSnapshotInfo> Articles { get; }
    public Dictionary<CustomerId, IdSnapshotInformation> Carts { get; }

    public ShoppingServiceSnapshotInfo(ShoppingServiceMock shoppingService, IIdRegistry idRegistry)
    {
        Customers = shoppingService.Customers.ToDictionary(
            customerKv => customerKv.Key,
            customerKv => new CustomerSnapshotInfo(customerKv.Value, idRegistry)
            );
        Orders = shoppingService.Orders.ToDictionary(
            orderKv => orderKv.Key,
            orderKv => new OrderSnapshotInfo(orderKv.Value, idRegistry)
        );
        OrderItems = shoppingService.OrderItems.ToDictionary(
            orderItemKv => orderItemKv.Key,
            orderItemKv => new OrderItemSnapshotInfo(orderItemKv.Value, idRegistry)
        );
        Articles = shoppingService.Articles.ToDictionary(
            articleKv => articleKv.Key,
            articleKv => new ArticleSnapshotInfo(articleKv.Value, idRegistry)
        );
        Carts = shoppingService.Carts.ToDictionary(
            cartIdKv => cartIdKv.Key,
            cartIdKv => cartIdKv.Value.IdSnapshot(idRegistry)
        );
    }
}