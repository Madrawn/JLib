﻿using FluentAssertions;
using JLib.AutoMapper;
using JLib.DataGeneration.Examples.Setup.Models;
using JLib.DataGeneration.Examples.Setup.SystemUnderTest;
using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snapshooter.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataGeneration.Examples.Getting_Started;
public sealed class UsingTheDefaultNamespace : IDisposable
{
    /*************************************************************\
    |                       Data Packages                         |
    \*************************************************************/
    public sealed class CustomerDp : DataPackage
    {
        public CustomerId CustomerId { get; set; } = null!;
        public CustomerDp(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            serviceProvider.GetRequiredServices(out ShoppingServiceMock shoppingService);
            shoppingService.AddCustomer(new(this.GetInfoText(x => x.CustomerId))
            {
                Id = CustomerId
            });
        }
    }
    #region
    /*************************************************************\
    |                           Setup                             |
    \*************************************************************/
    private readonly List<IDisposable> _disposables = new();
    private readonly ShoppingServiceMock _shoppingService;

    public UsingTheDefaultNamespace(ITestOutputHelper testOutputHelper)
    {
        using var exceptions = new ExceptionBuilder("setup");

        var loggerFactory = new LoggerFactory().AddXunit(testOutputHelper);

        var serviceCollection = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, loggerFactory,
                JLibDataGenerationTp.Instance,
                TypePackage.GetNested<UsingTheDefaultNamespace>())
            .AddSingleton<ShoppingServiceMock>()
            .AddScopedAlias<IShoppingService, ShoppingServiceMock>()
            .AddAutoMapper(b => b.AddProfiles(typeCache, loggerFactory))
            .AddDataPackages(typeCache, new()
            {
                DefaultNamespace = $"{typeof(UsingTheDefaultNamespace).Namespace}.{nameof(UsingTheDefaultNamespace)}"
            });

        var serviceProvider = serviceCollection
            .BuildServiceProvider()
            .DisposeWith(_disposables)
            .IncludeDataPackages<CustomerDp>();
        _shoppingService = serviceProvider.GetRequiredService<ShoppingServiceMock>();
    }
    public void Dispose()
        => _disposables.DisposeAll();


    /*************************************************************\
    |                            Test                             |
    \*************************************************************/
    [Fact]
    public void Test()
    {
        var id = _shoppingService.Customers.Single().Key;
        id.IdInfo().Should().Be($"CustomerId [CustomerDp].[CustomerId] = {id.Value}");
        _shoppingService.Customers.MatchSnapshot();
    }

    #endregion
}
