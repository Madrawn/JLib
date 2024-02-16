using JLib.DataGeneration.Examples.Setup.Models;
using JLib.DataGeneration.Examples.Setup.SystemUnderTest;

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace JLib.DataGeneration.Examples.DataPackages;

public interface ICustomerDp
{
    public CustomerId Customer { get; }

}

public abstract class CustomerDpb : DataPackage, ICustomerDp
{
    public CustomerId Customer { get; init; }

    protected CustomerDpb(IDataPackageManager packageManager, ShoppingServiceMock shoppingService) : base(packageManager)
    {
        shoppingService.AddCustomer(
            new CustomerEntity(nameof(Customer))
            {
                Id = Customer
            }
        );
    }

}
public sealed class CustomerDp : CustomerDpb
{
    public CustomerDp(IDataPackageManager packageManager, ShoppingServiceMock shoppingService) : base(packageManager, shoppingService)
    {
    }
}
public sealed class OtherCustomerDp : CustomerDpb
{
    public OtherCustomerDp(IDataPackageManager packageManager, ShoppingServiceMock shoppingService) : base(packageManager, shoppingService)
    {
    }
}