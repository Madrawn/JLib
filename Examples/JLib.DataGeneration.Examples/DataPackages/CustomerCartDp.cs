using JLib.DataGeneration.Examples.Models;
using JLib.DataGeneration.Examples.SystemUnderTest;
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace JLib.DataGeneration.Examples.DataPackages;

public interface IOrderDp
{
    public OrderId Order { get; }
}

public sealed class CustomerCartDp<TCustomerDp> : DataPackage, IOrderDp
    where TCustomerDp : ICustomerDp
{
    public OrderId Order { get; init; }

    public CustomerCartDp(
            IDataPackageManager packageManager, ShoppingServiceMock shoppingService, TCustomerDp customerDp
        ) : base(packageManager)
    {
        shoppingService.AddOrders(
            new OrderEntity(customerDp.Customer, OrderStatus.Cart)
            {
                Id = Order
            }
        );
        shoppingService.SetCart(customerDp.Customer, Order);
    }
}