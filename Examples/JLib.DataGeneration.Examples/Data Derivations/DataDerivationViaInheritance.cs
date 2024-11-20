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


namespace JLib.DataGeneration.Examples.Data_Derivations;
public sealed class DataDerivationViaInheritance : IDisposable
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

    public DataDerivationViaInheritance(ITestOutputHelper testOutputHelper)
    {
        var exceptions = new ExceptionBuilder("setup");
        var loggerFactory = new LoggerFactory().AddXunit(testOutputHelper);

        var serviceCollection = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, loggerFactory,
                JLibDataGenerationTp.Instance,
                TypePackage.GetNested<DataDerivationViaInheritance>())
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
