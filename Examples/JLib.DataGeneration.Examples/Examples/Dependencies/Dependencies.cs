using FluentAssertions;
using JLib.DataGeneration.Examples.Setup.Models;
using JLib.DataGeneration.Examples.Setup.SystemUnderTest;
using JLib.Exceptions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JLib.DataGeneration.Examples.Dependencies;

public sealed class MinimalCode : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly ShoppingServiceMock _shoppingService;

    public MinimalCode()
    {
        var exceptions = new ExceptionManager("setup");

        var services = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, JLibDataGenerationExamplesTypePackage.Instance)
            .AddDataPackages(typeCache)
            .AddAutoMapper(m => m.AddProfiles(typeCache))
            .AddSingleton<ShoppingServiceMock>()
            .AddAlias<IShoppingService, ShoppingServiceMock>(ServiceLifetime.Singleton);

        _provider = services.BuildServiceProvider();
        _shoppingService = _provider.GetRequiredService<ShoppingServiceMock>();

        exceptions.ThrowIfNotEmpty();
    }

    [Fact]
    public void Test()
    {
        // all dependencies will be added automatically
        _provider.IncludeDataPackages<CustomerOrdersDp>();

        _shoppingService.Customers.Should().HaveCount(1);
        _shoppingService.Orders.Should().HaveCount(2);
    }

    public void Dispose() 
        => _provider.Dispose();
}

public sealed class CustomerDp : DataPackage
{
    public CustomerId Customer { get; init; } = default!;

    public CustomerDp(IDataPackageManager packageManager, ShoppingServiceMock shoppingService) : base(packageManager)
    {
        shoppingService.AddCustomer(
            new CustomerEntity(nameof(Customer))
            {
                Id = Customer
            }
        );
    }
}

public sealed class CustomerOrdersDp : DataPackage
{
    public OrderId Fulfilled { get; init; } = default!;
    public OrderId Cart { get; init; } = default!;

    public CustomerOrdersDp(
        IDataPackageManager packageManager, ShoppingServiceMock shoppingService, 
        // to use another DataPackage, you can inject it just like other services
        CustomerDp customer
    ) : base(packageManager)
    {
        shoppingService.AddOrders(
            new OrderEntity(customer.Customer, OrderStatus.Cart)
            {
                Id = Cart
            }
        );
        shoppingService.SetCart(customer.Customer, Cart);

        shoppingService.AddOrders(
            new OrderEntity(customer.Customer, OrderStatus.Fulfilled)
            {
                Id = Fulfilled
            }
        );
    }
}