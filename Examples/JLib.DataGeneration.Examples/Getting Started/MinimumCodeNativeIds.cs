// Third party packages
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snapshooter.Xunit;
using Xunit;
using Xunit.Abstractions;

// required JLib packages
using JLib.AutoMapper;
using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;

// referenced setup
using JLib.DataGeneration.Examples.Setup.Models;
using JLib.DataGeneration.Examples.Setup.SystemUnderTest;
using JLib.Reflection.DependencyInjection;
using JLib.ValueTypes;

namespace JLib.DataGeneration.Examples.Getting_Started;

/// <summary>
/// This example contains the minimum code required to use a <see cref="DataPackage"/> without <see cref="ValueType{T}"/> Ids (i.e. <see cref="GuidValueType"/>)
/// </summary>
public sealed class MinimumCodeNativeIds : IDisposable
{
    /*************************************************************\
    |                       Data Packages                         |
    \*************************************************************/
    public sealed class CustomerDp : DataPackage
    {
        public Guid CustomerId { get; set; } = default!;
        public CustomerDp(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            serviceProvider.GetRequiredServices(out ShoppingServiceMock shoppingService);
            shoppingService.AddCustomer(new(GetInfoText(nameof(CustomerId)))
            {
                Id = new CustomerId(CustomerId)
            });
        }
    }

    /*************************************************************\
    |                           Setup                             |
    \*************************************************************/
    private readonly List<IDisposable> _disposables = new();
    private readonly ShoppingServiceMock _shoppingService;

    public MinimumCodeNativeIds(ITestOutputHelper testOutputHelper)
    {
        IServiceCollection serviceCollection;

        using (var exceptions = new ExceptionBuilder("setup"))
        {
            var loggerFactory = new LoggerFactory().AddXunit(testOutputHelper);


            serviceCollection = new ServiceCollection()
                .AddTypeCache(out var typeCache, exceptions, loggerFactory,
                    JLibDataGenerationTp.Instance,
                    TypePackage.GetNested<MinimumCodeNativeIds>())
                .AddSingleton<ShoppingServiceMock>()
                .AddScopedAlias<IShoppingService, ShoppingServiceMock>()
                .AddAutoMapper(b => b.AddProfiles(typeCache, loggerFactory))
                .AddDataPackages(typeCache);
        }

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
        => _shoppingService.Customers.MatchSnapshot();
}
