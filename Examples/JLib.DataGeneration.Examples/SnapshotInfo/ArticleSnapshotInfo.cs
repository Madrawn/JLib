using JLib.DataGeneration.Examples.Setup.Models;

namespace JLib.DataGeneration.Examples.SnapshotInfo;

public class ArticleSnapshotInfo
{
    public ArticleSnapshotInfo(ArticleEntity article, IIdRegistry idRegistry)
    {
        Id = article.Id.IdSnapshot(idRegistry);
        Name = article.Name;
        Price = article.Price;
    }
    public IdSnapshotInformation Id { get; init; }
    public string Name { get; init; }
    public double Price { get; init; }
}

public class OrderItemSnapshotInfo
{
    public OrderItemSnapshotInfo(OrderItemEntity orderItem, IIdRegistry idRegistry)
    {
        Id = orderItem.Id.IdSnapshot(idRegistry);
        OrderId = orderItem.OrderId.IdSnapshot(idRegistry);
        ArticleId = orderItem.ArticleId.IdSnapshot(idRegistry);
        Quantity = orderItem.Quantity;
        Price = orderItem.Price;
    }
    public IdSnapshotInformation Id { get; init; }
    public IdSnapshotInformation OrderId { get; init; }
    public IdSnapshotInformation ArticleId { get; init; }
    public int Quantity { get; init; }
    public double Price { get; init; }
}