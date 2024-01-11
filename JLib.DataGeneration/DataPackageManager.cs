using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.DataGeneration;

public interface IDataPackageManager
{
    internal DataPackageInitState InitState { get; }

    internal void IncludeDataPackages(DataPackageType[] packages);
    internal void SetIdPropertyValue(object packageInstance, PropertyInfo property);
}
internal class DataPackageManager : IDataPackageManager
{
    private readonly IIdRegistry _idRegistry;
    private readonly IServiceProvider _provider;

    public DataPackageManager(IIdRegistry idRegistry, IServiceProvider provider)
    {
        _idRegistry = idRegistry;
        _provider = provider;
    }
    public DataPackageInitState InitState { get; private set; }
    public void IncludeDataPackages(DataPackageType[] packages)
    {
        if (InitState != DataPackageInitState.Uninitialized)
            throw new InvalidOperationException($"dataPackages are {InitState} and cannot be loaded again.");

        InitState = DataPackageInitState.Initializing;

        foreach (var dataPackageType in packages)
            _provider.GetRequiredService(dataPackageType.Value);
        InitState = DataPackageInitState.Initialized;
        _idRegistry.SaveToFile();
    }

    public void SetIdPropertyValue(object packageInstance, PropertyInfo property)
        => _idRegistry.SetIdPropertyValue(packageInstance, property);
}