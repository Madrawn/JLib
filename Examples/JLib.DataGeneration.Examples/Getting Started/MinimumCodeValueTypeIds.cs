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

namespace JLib.DataGeneration.Examples.Getting_Started;
public sealed class MinimumCodeValueTypeIds : IDisposable
{
    /*************************************************************\
    |                       Data Packages                         |
    \*************************************************************/
    public sealed class CustomerDp : DataPackage
    {
        public CustomerId CustomerId { get; set; } = null!;
        public CustomerDp(ShoppingServiceMock shoppingService, IDataPackageManager packageManager) : base(packageManager)
        {
            shoppingService.AddCustomer(new(GetInfoText(nameof(CustomerId)))
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

    public MinimumCodeValueTypeIds(ITestOutputHelper testOutputHelper)
    {
        using var exceptions = new ExceptionBuilder("setup");

        var loggerFactory = new LoggerFactory().AddXunit(testOutputHelper);

        var serviceCollection = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, loggerFactory,
                JLibDataGenerationTp.Instance,
                TypePackage.GetNested<MinimumCodeValueTypeIds>())
            .AddSingleton<ShoppingServiceMock>()
            .AddScopedAlias<IShoppingService, ShoppingServiceMock>()
            .AddAutoMapper(b => b.AddProfiles(typeCache, loggerFactory))
            .AddDataPackages(typeCache);

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
    #endregion
}
