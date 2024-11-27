using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.DataGeneration;

/// <summary>
/// Manages the initialization of <see cref="DataPackage"/>s
/// </summary>
internal sealed class DataPackageManager 
{
    private readonly IIdRegistry _idRegistry;
    private readonly IServiceProvider _provider;
    public DataPackageManager(IIdRegistry idRegistry, IServiceProvider provider, IdRegistryConfiguration configuration)
    {
        _idRegistry = idRegistry;
        _provider = provider;
        Configuration = configuration;
    }

    public DataPackageInitState InitState { get; private set; }

    public IdRegistryConfiguration Configuration { get; }

    public void IncludeDataPackages(Type[] packages)
    {
        if (InitState != DataPackageInitState.Uninitialized)
            throw new InvalidOperationException($"dataPackages are {InitState} and cannot be loaded again.");

        packages.Where(p => !p.IsAssignableTo<DataPackage>() || p.IsAbstract)
            .Select(p =>
                new InvalidSetupException(p.FullName() +
                                          " is not a valid typePackage. it must not be abstract and be Derived from DataPackage"))
            .ThrowExceptionIfNotEmpty("invalid DataPackages found");

        InitState = DataPackageInitState.Initializing;

        foreach (var dataPackageType in packages)
            _provider.GetRequiredService(dataPackageType);
        InitState = DataPackageInitState.Initialized;
    }

    public void SetIdPropertyValue(object packageInstance, PropertyInfo property)
        => _idRegistry.SetIdPropertyValue(packageInstance, property);
}