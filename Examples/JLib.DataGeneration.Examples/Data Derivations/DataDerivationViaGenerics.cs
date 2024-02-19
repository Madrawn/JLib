﻿// Third party packages
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snapshooter.Xunit;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;

// required JLib packages
using JLib.AutoMapper;
using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;

// referenced setup
using JLib.DataGeneration.Examples.Setup.Models;
using JLib.DataGeneration.Examples.Setup.SystemUnderTest;

namespace JLib.DataGeneration.Examples.Data_Derivations;
public sealed class DataDerivationViaGenerics : IDisposable
{
    /*************************************************************\
    |                       Data Packages                         |
    \*************************************************************/
    public abstract class CustomerDpb : DataPackage
    {
        public CustomerId Id { get; set; } = null!;
        protected CustomerDpb(ShoppingServiceMock shoppingService, IDataPackageManager packageManager) : base(packageManager)
        {
            shoppingService.AddCustomer(new(GetInfoText(nameof(Id)))
            {
                Id = Id
            });
        }
    }

    public sealed class CustomerDp : CustomerDpb
    {
        public CustomerDp(ShoppingServiceMock shoppingService, IDataPackageManager packageManager) : base(shoppingService, packageManager)
        {
        }
    }

    public sealed class OtherCustomerDp : CustomerDpb
    {
        public OtherCustomerDp(ShoppingServiceMock shoppingService, IDataPackageManager packageManager) : base(shoppingService, packageManager)
        {
        }
    }

    public sealed class OrderDp<TCustomerDp> : DataPackage
        where TCustomerDp : CustomerDpb
    {
        public OrderId Id { get; init; } = null!;
        public OrderDp(TCustomerDp customerDp, ShoppingServiceMock shoppingServiceMock, IDataPackageManager packageManager) : base(packageManager)
        {
            shoppingServiceMock.AddOrders(new OrderEntity(customerDp.Id, OrderStatus.Fulfilled)
            {
                Id = Id,
            });
        }
    }

    /*************************************************************\
    |                      Snapshot Infos                         |
    \*************************************************************/
    public class CustomerSnapshotInfo
    {
        public IdSnapshotInformation Id { get; }
        public string UserName { get; }
        public CustomerSnapshotInfo(CustomerEntity customer, IIdRegistry idRegistry)
        {
            Id = customer.Id.IdSnapshot(idRegistry);
            UserName = customer.UserName;
        }
    }

    public class OrderSnapshotInfo
    {
        public IdSnapshotInformation Id { get; }
        public IdSnapshotInformation CustomerId { get; }
        public OrderStatus Status { get; }
        public OrderSnapshotInfo(OrderEntity order, IIdRegistry idRegistry)
        {
            Id = order.Id.IdSnapshot(idRegistry);
            CustomerId = order.Customer.IdSnapshot(idRegistry);
            Status = order.Status;
        }
    }

    public class ShoppingServiceSnapshotInfo
    {
        public IReadOnlyCollection<CustomerSnapshotInfo> Customers { get; }
        public IReadOnlyCollection<OrderSnapshotInfo> Orders { get; }

        public ShoppingServiceSnapshotInfo(ShoppingServiceMock service, IIdRegistry idRegistry)
        {
            Customers = service.Customers.Values.Select(customer => new CustomerSnapshotInfo(customer, idRegistry)).ToList();
            Orders = service.Orders.Values.Select(order => new OrderSnapshotInfo(order, idRegistry)).ToList();
        }
    }

    #region
    /*************************************************************\
    |                           Setup                             |
    \*************************************************************/
    private readonly List<IDisposable> _disposables = new();
    private readonly ShoppingServiceMock _shoppingService;
    private readonly IIdRegistry _idRegistry;

    public DataDerivationViaGenerics(ITestOutputHelper testOutputHelper)
    {
        var exceptions = new ExceptionBuilder("setup");
        var loggerFactory = new LoggerFactory().AddXunit(testOutputHelper);

        var serviceCollection = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, loggerFactory,
                JLibDataGenerationTp.Instance,
                TypePackage.GetNested<DataDerivationViaGenerics>())
            .AddSingleton<ShoppingServiceMock>()
            .AddScopedAlias<IShoppingService, ShoppingServiceMock>()
            .AddAutoMapper(b => b.AddProfiles(typeCache, loggerFactory))
            .AddDataPackages(typeCache);

        exceptions.ThrowIfNotEmpty();

        var serviceProvider = serviceCollection
            .BuildServiceProvider()
            .DisposeWith(_disposables)
            .IncludeDataPackages<OrderDp<CustomerDp>, OrderDp<OtherCustomerDp>>();

        _shoppingService = serviceProvider.GetRequiredService<ShoppingServiceMock>();
        _idRegistry = serviceProvider.GetRequiredService<IIdRegistry>();
    }
    public void Dispose()
        => _disposables.DisposeAll();

    #endregion

    /*************************************************************\
    |                            Test                             |
    \*************************************************************/
    [Fact]
    public void SnapshotTest()
        => new ShoppingServiceSnapshotInfo(_shoppingService, _idRegistry).MatchSnapshot();

    [Fact]
    public void ManualTest()
    {
        _shoppingService.Orders.Count.Should().Be(2);
        _shoppingService.Customers.Count.Should().Be(2);
    }
}
