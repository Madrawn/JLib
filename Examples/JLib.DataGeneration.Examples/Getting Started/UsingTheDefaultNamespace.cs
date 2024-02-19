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
/* Unmerged change from project 'JLib.DataGeneration.Examples (net6.0)'
Before:
using JLib.DataGeneration.Examples.Setup.SystemUnderTest;
After:
using JLib.DataGeneration.Examples.Setup.SystemUnderTest;
using JLib;
using JLib.DataGeneration;
using JLib.DataGeneration.Examples;
using JLib.DataGeneration.Examples.EntryLevel;
*/

namespace JLib.DataGeneration.Examples.Getting_Started;
#endregion
public sealed class UsingTheDefaultNamespace : IDisposable
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
