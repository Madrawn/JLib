#region
// Third party packages
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Snapshooter.Xunit;

// required JLib packages
using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.AutoMapper;

// referenced example setup code
using JLib.DataGeneration.Examples.Setup.Models;
using JLib.DataGeneration.Examples.Setup.SystemUnderTest;

namespace JLib.DataGeneration.Examples;
#endregion
public sealed class SnapshotInfoExample : IDisposable
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

    #region
    /*************************************************************\
    |                           Setup                             |
    \*************************************************************/
    private readonly List<IDisposable> _disposables = new();
    private readonly ShoppingServiceMock _shoppingService;
    private readonly IIdRegistry _idRegistry;

    public SnapshotInfoExample(ITestOutputHelper testOutputHelper)
    {
        var exceptions = new ExceptionBuilder("setup");
        var loggerFactory = new LoggerFactory().AddXunit(testOutputHelper);

        var serviceCollection = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, loggerFactory,
                JLibDataGenerationTp.Instance,
                TypePackage.GetNested<SnapshotInfoExample>())
            .AddSingleton<ShoppingServiceMock>()
            .AddScopedAlias<IShoppingService, ShoppingServiceMock>()
            .AddAutoMapper(b => b.AddProfiles(typeCache, loggerFactory))
            .AddDataPackages(typeCache);

        exceptions.ThrowIfNotEmpty();

        var serviceProvider = serviceCollection
            .BuildServiceProvider()
            .DisposeWith(_disposables)
            .IncludeDataPackages<CustomerDp>();

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
    public void Test()
        => _shoppingService.Customers
            .Values
            .Select(customer => new CustomerSnapshotInfo(customer, _idRegistry))
            .MatchSnapshot();
}
