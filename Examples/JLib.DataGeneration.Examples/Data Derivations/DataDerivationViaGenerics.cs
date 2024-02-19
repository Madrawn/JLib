// todo
#region
// Third party packages

using FluentAssertions;
using JLib.AutoMapper;
using JLib.DataGeneration.Examples.Setup.Models;
using JLib.DataGeneration.Examples.Setup.SystemUnderTest;
using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snapshooter.Xunit;
using Xunit;
using Xunit.Abstractions;
// required JLib packages

// referenced example setup code

namespace JLib.DataGeneration.Examples.Data_Derivations;
#endregion
public sealed class DataDerivationViaGenerics : IDisposable
{
    /*************************************************************\
    |                       Data Packages                         |
    \*************************************************************/
    public abstract class CustomerDpb : DataPackage
    {
        public CustomerId CustomerId { get; set; } = null!;
        protected CustomerDpb(ShoppingServiceMock shoppingService, IDataPackageManager packageManager) : base(packageManager)
        {
            shoppingService.AddCustomer(new(GetInfoText(nameof(CustomerId)))
            {
                Id = CustomerId
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
            .IncludeDataPackages<CustomerDp, OtherCustomerDp>();

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
            .Should().HaveCount(2)
            .And.Subject
            .MatchSnapshot();
}
