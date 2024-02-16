using FluentAssertions;
using JLib.AutoMapper;
using JLib.DataGeneration.Examples.Setup.Models;
using JLib.DataGeneration.Examples.Setup.SystemUnderTest;
using JLib.DependencyInjection;
using JLib.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataGeneration.Examples.ReusingData;

public sealed class MinimalCode : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly ShoppingServiceMock _shoppingService;

    public MinimalCode(ITestOutputHelper testOutputHelper)
    {
        var exceptions = ExceptionBuilder.Create("setup");

        var loggerFactory = new LoggerFactory()
            .AddXunit(testOutputHelper);

        var services = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, loggerFactory, JLibDataGenerationExamplesTypePackage.Instance)
            .AddDataPackages(typeCache)
            .AddAutoMapper(m => m.AddProfiles(typeCache, loggerFactory))
            .AddSingleton<ShoppingServiceMock>()
            .AddAlias<IShoppingService, ShoppingServiceMock>(ServiceLifetime.Singleton);

        _provider = services.BuildServiceProvider();
        _shoppingService = _provider.GetRequiredService<ShoppingServiceMock>();

        exceptions.ThrowIfNotEmpty();
    }

    [Fact]
    public void Test()
    {
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

public abstract class CustomerOrdersDpb : DataPackage
{
    protected CustomerOrdersDpb(IDataPackageManager packageManager) : base(packageManager)
    {
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