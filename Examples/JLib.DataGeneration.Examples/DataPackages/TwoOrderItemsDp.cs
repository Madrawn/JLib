using JLib.DataGeneration.Examples.Models;
using JLib.DataGeneration.Examples.SystemUnderTest;

namespace JLib.DataGeneration.Examples.DataPackages;

#pragma warning disable CS8604, CS8601, CS8618
public sealed class TwoOrderItemsDp<TOrderDp> : DataPackage
    where TOrderDp : IOrderDp
{
    public OrderItemId FirstOrderItem { get; init; }
    public OrderItemId SecondOrderItem { get; init; }

    public TwoOrderItemsDp(IDataPackageManager packageManager, ShoppingServiceMock shoppingService, TOrderDp order, ArticleDp articles) : base(packageManager)
    {
        shoppingService.AddOrderItems(
            new OrderItemEntity(order.Order, articles.FirstArticle, 2, 10)
            {
                Id = FirstOrderItem
            },
            new OrderItemEntity(order.Order, articles.SecondArticle, 2, 20)
            {
                Id = SecondOrderItem
            }
        );
    }
}