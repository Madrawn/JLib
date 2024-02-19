#region
// Third party packages

using FluentAssertions;
using JLib.AutoMapper;
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

public sealed class DebugUtilities : IDisposable
{

    #region

    /*************************************************************\
    |                           Setup                             |
    \*************************************************************/
    private readonly List<IDisposable> _disposables = new();
    private readonly IIdRegistry _idRegistry;

    public DebugUtilities(ITestOutputHelper testOutputHelper)
    {
        var exceptions = new ExceptionBuilder("setup");
        var loggerFactory = new LoggerFactory().AddXunit(testOutputHelper);

        var serviceCollection = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, loggerFactory,
                JLibDataGenerationTp.Instance,
                TypePackage.GetNested<DebugUtilities>())
            .AddSingleton<ShoppingServiceMock>()
            .AddScopedAlias<IShoppingService, ShoppingServiceMock>()
            .AddAutoMapper(b => b.AddProfiles(typeCache, loggerFactory))
            .AddDataPackages(typeCache);

        exceptions.ThrowIfNotEmpty();

        var serviceProvider = serviceCollection
            .BuildServiceProvider()
            .DisposeWith(_disposables)
            .IncludeDataPackages(Array.Empty<Type>());

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
    {
        DataPackageValues.IdGroupName groupName = new(nameof(GenerateIdsManually));
        DataPackageValues.IdName name = new(nameof(Test));
        DataPackageValues.IdIdentifier identifier = new(groupName, name);
        var id = _idRegistry.GetGuidId(identifier);

        // recommended for building snapshot Infos
        id.IdSnapshot(_idRegistry).Should().Be(new IdSnapshotInformation(identifier, id));

        // recommended for debugging purposes only
        id.IdInfo().Should().Be($"Guid [{groupName.Value}].[{name.Value}] = {id}");
        id.IdInfoObj().Should().Be(new IdInformation(typeof(Guid), identifier, id));
    }
}