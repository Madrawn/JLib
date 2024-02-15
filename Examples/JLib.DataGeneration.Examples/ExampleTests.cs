using FluentAssertions;
using JLib.DataGeneration.Examples.DataPackages;
using JLib.DataGeneration.Examples.Setup.Models;
using JLib.DataGeneration.Examples.Setup.SystemUnderTest;
using JLib.DataGeneration.Examples.SnapshotInfo;
using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace JLib.DataGeneration.Examples;

public class ExampleTests : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly IServiceProvider _provider;
    private readonly ShoppingServiceMock _shoppingService;
    private readonly IIdRegistry _idRegistry;

    public ExampleTests(ITestOutputHelper testOutputHelper)
    {
        var exceptions = ExceptionBuilder.Create("setup");

        var loggerFactory = new LoggerFactory()
            .AddXunit(testOutputHelper);

        var services = new ServiceCollection()
            // executes and caches reflection on all given types
            .AddTypeCache(out var typeCache, exceptions, loggerFactory, JLibDataGenerationExamplesTypePackage.Instance)
            // add all type packages
            .AddDataPackages(typeCache, new() { DefaultNamespace = "JLib.DataGeneration.Examples.DataPackages" })
            // add all automapper profiles (required profile: JLib.AutoMapper.ValueTypeProfile)
            .AddAutoMapper(m => m.AddProfiles(typeCache, loggerFactory))
            .AddSingleton<ShoppingServiceMock>()
            .AddAlias<IShoppingService, ShoppingServiceMock>(ServiceLifetime.Singleton);

        var provider = services.BuildServiceProvider();
        _provider = provider;
        _shoppingService = provider.GetRequiredService<ShoppingServiceMock>();
        _idRegistry = provider.GetRequiredService<IIdRegistry>();
        _disposables.Add(provider);
        exceptions.ThrowIfNotEmpty();
    }
    [Fact]
    public void Version1_ManualDataNaive()
    {
        CustomerId customerId = new(Guid.NewGuid());
        _shoppingService.AddCustomer(
            new CustomerEntity("Customer")
            {
                Id = customerId
            }
        );
        OrderId orderId = new(Guid.NewGuid());
        _shoppingService.AddOrders(
            new OrderEntity(customerId, OrderStatus.Cart)
            {
                Id = orderId
            }
        );
        _shoppingService.SetCart(customerId, orderId);
        ArticleId firstArticle = new(Guid.NewGuid());
        ArticleId secondArticle = new(Guid.NewGuid());
        ArticleId thirdArticle = new(Guid.NewGuid());
        ArticleId fourthArticle = new(Guid.NewGuid());
        _shoppingService.AddArticles(
            new(nameof(firstArticle), 10)
            {
                Id = firstArticle
            },
            new(nameof(secondArticle), 20.20)
            {
                Id = secondArticle
            },
            new(nameof(thirdArticle), 30)
            {
                Id = thirdArticle
            },
            new(nameof(fourthArticle), 40.40)
            {
                Id = fourthArticle
            }
        );
        OrderItemId firstOrderItemId = new(Guid.NewGuid());
        OrderItemId secondOrderItemId = new(Guid.NewGuid());
        _shoppingService.AddOrderItems(
                new OrderItemEntity(orderId, firstArticle, 2, 10)
                {
                    Id = firstOrderItemId
                },
                new OrderItemEntity(orderId, secondArticle, 2, 20)
                {
                    Id = secondOrderItemId
                }
            );

        _shoppingService.AddArticleToCart(customerId, thirdArticle, 10);

        Action testFails = () => _shoppingService.MatchSnapshot();
        testFails.Should().Throw<EqualException>();
    }
    [Fact]
    public void Version1_ManualData()
    {
        CustomerId customerId = new(Guid.Parse("00000001-0000-0000-0000-000000000000"));
        _shoppingService.AddCustomer(
            new CustomerEntity("Customer")
            {
                Id = customerId
            }
        );
        OrderId orderId = new(Guid.Parse("00000002-0000-0000-0000-000000000000"));
        _shoppingService.AddOrders(
            new OrderEntity(customerId, OrderStatus.Cart)
            {
                Id = orderId
            }
        );
        _shoppingService.SetCart(customerId, orderId);
        ArticleId firstArticle = new(Guid.Parse("00000003-0000-0000-0000-000000000000"));
        ArticleId secondArticle = new(Guid.Parse("00000003-0001-0000-0000-000000000000"));
        ArticleId thirdArticle = new(Guid.Parse("00000003-0002-0000-0000-000000000000"));
        ArticleId fourthArticle = new(Guid.Parse("00000003-0003-0000-0000-000000000000"));
        _shoppingService.AddArticles(
            new(nameof(firstArticle), 10)
            {
                Id = firstArticle
            },
            new(nameof(secondArticle), 20.20)
            {
                Id = secondArticle
            },
            new(nameof(thirdArticle), 30)
            {
                Id = thirdArticle
            },
            new(nameof(fourthArticle), 40.40)
            {
                Id = fourthArticle
            }
        );
        OrderItemId firstOrderItemId = new(Guid.Parse("00000004-0000-0000-0000-000000000000"));
        OrderItemId secondOrderItemId = new(Guid.Parse("00000004-0001-0000-0000-000000000000"));
        _shoppingService.AddOrderItems(
                new OrderItemEntity(orderId, firstArticle, 2, 10)
                {
                    Id = firstOrderItemId
                },
                new OrderItemEntity(orderId, secondArticle, 2, 20)
                {
                    Id = secondOrderItemId
                }
            );

        _shoppingService.AddArticleToCart(customerId, thirdArticle, 10);

        _shoppingService.MatchSnapshot();
    }
    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public void Version2_NaiveWay()
    {
        _provider.IncludeDataPackages<TwoOrderItemsDp<CustomerCartDp<CustomerDp>>>();

        _provider.GetRequiredServices(out CustomerDp customer, out ArticleDp articles);
        _shoppingService.AddArticleToCart(customer.Customer, articles.ThirdArticle, 10);

        _shoppingService.MatchSnapshot();
    }
    [Fact]
    public void Version3_UsingSnapshotInfos()
    {
        _provider.IncludeDataPackages<TwoOrderItemsDp<CustomerCartDp<CustomerDp>>>();

        _provider.GetRequiredServices(out CustomerDp customer, out ArticleDp articles);
        _shoppingService.AddArticleToCart(customer.Customer, articles.ThirdArticle, 10);

        new ShoppingServiceSnapshotInfo(_shoppingService, _idRegistry).MatchSnapshot();
    }
    [Fact]
    public void Version4_AddingBeforeAfterCompare()
    {
        _provider.IncludeDataPackages<TwoOrderItemsDp<CustomerCartDp<CustomerDp>>>();

        _provider.GetRequiredServices(out CustomerDp customer, out ArticleDp articles);

        new ShoppingServiceSnapshotInfo(_shoppingService, _idRegistry).MatchSnapshot(new SnapshotNameExtension("setup"));

        _shoppingService.AddArticleToCart(customer.Customer, articles.ThirdArticle, 10);

        new ShoppingServiceSnapshotInfo(_shoppingService, _idRegistry).MatchSnapshot(new SnapshotNameExtension("result"));
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
            disposable.Dispose();
    }
}