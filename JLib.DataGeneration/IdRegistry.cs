using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using AutoMapper;
using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;
using Microsoft.Extensions.DependencyInjection;
using static JLib.DataGeneration.DataPackageValues;

namespace JLib.DataGeneration;

public static class DataPackageExtensions
{
    public static IServiceCollection AddDataPackages(this IServiceCollection services, ITypeCache typeCache)
    {
        services.AddSingleton<IDataPackageManager, DataPackageManager>();
        services.AddSingleton(typeCache.All<DataPackageType>().Select(t => t.Value));
        return services;
    }

    public static IServiceProvider IncludeDataPackages(this IServiceProvider provider, params DataPackageType[] packages)
    {
        provider.GetRequiredService<IDataPackageManager>().IncludeDataPackages();
        return provider;
    }
}

internal enum DataPackageInitState
{
    Uninitialized,
    Initializing,
    Initialized
}

public interface IDataPackageManager
{
    internal DataPackageInitState InitState { get; }

    public void IncludeDataPackages();
    internal void SetIdPropertyValue(PropertyInfo property);
}

internal class DataPackageManager : IDataPackageManager
{
    private readonly IIdRegistry _idRegistry;

    public DataPackageManager(IIdRegistry idRegistry, ITypeCache)
    {
        _idRegistry = idRegistry;
    }
    public DataPackageInitState InitState { get; private set; }
    public void IncludeDataPackages()
    {
        if (InitState != DataPackageInitState.Uninitialized)
            throw new InvalidOperationException($"dataPackages are {InitState} and cannot be loaded again.");

        InitState = DataPackageInitState.Initialized;

        foreach (var dataPackageType in packages)
            provider.GetRequiredService(dataPackageType.Value);
    }
}

public interface IIdRegistry
{
    internal void SetIdPropertyValue(PropertyInfo property);
    internal void SaveToFile();
}

internal class IdRegistry : IIdRegistry
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly IdIdentifier IncrementIdentifier = new(new("__registry__"), new("IdIncrement"));

    private readonly Lazy<IMapper> _mapper;
    private readonly string _fileLocation;
    private readonly Dictionary<IdIdentifier, object> _dictionary;
    private bool _isDirty;
    private int _idIncrement;
    public IdRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _fileLocation = GetFileName();
        _dictionary = LoadFromFile();
        _idIncrement = _dictionary.GetValueOrDefault(IncrementIdentifier) as int? ?? 1;
        _mapper = new Lazy<IMapper>(() => _serviceProvider.GetRequiredService<IMapper>());
    }

    private static string GetFileName()
    {
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        var targetDir = Parent(currentDir);
        var file = Path.Combine(targetDir, "DataPackageStore.json");
        return file;

        static string Parent(string dir)
        {
            while (Directory.GetFiles(dir, "*.csproj").None())
            {
                dir = Directory.GetParent(dir)?.FullName!;
                if (dir is null)
                    throw new InvalidOperationException("no directory with csproj file found");
            }

            return dir;
        }
    }
    /// <summary>
    /// returns a deterministic <see cref="Guid"/> value for the given <paramref name="identifier"/>
    /// </summary>
    public Guid GetGuidId(IdIdentifier identifier)
        => _dictionary.GetValueOrAdd(identifier, () =>
        {
            _isDirty = true;
            return Guid.NewGuid();
        }).CastTo<Guid>();
    /// <summary>
    /// returns a deterministic <see cref="int"/> value for the given <paramref name="identifier"/>
    /// </summary>
    public int GetIntId(IdIdentifier identifier)
        => _dictionary.GetValueOrAdd(identifier, () =>
        {
            _isDirty = true;
            return _idIncrement++;
        }).CastTo<int>();

    /// <summary>
    /// sets the id of the given <paramref name="property"/> to the persisted id or creates a new one<br/>
    /// throws a <see cref="ArgumentOutOfRangeException"/> when the property is neither a <see cref="int"/>, <see cref="Guid"/>, <see cref="IntValueType"/> nor <see cref="GuidValueType"/>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    void IIdRegistry.SetIdPropertyValue(PropertyInfo property)
    {
        var packageType = property.ReflectedType
                          ?? throw new Exception("Property has no Reflected type");

        var packageInstance = _serviceProvider.GetRequiredService(packageType);

        var id = GetId(new(property), property.PropertyType);
        property.SetValue(packageInstance, id);
    }
    /// <summary>
    /// gets the id with the given <paramref name="identifier"/> of the given <paramref name="idType"/><br/>
    /// throws a <see cref="ArgumentOutOfRangeException"/> when the property is neither a <see cref="int"/>, <see cref="Guid"/>, <see cref="IntValueType"/> nor <see cref="GuidValueType"/>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private object GetId(IdIdentifier identifier, Type idType)
    {
        if (idType == typeof(int))
            return GetIntId(identifier);
        if (idType.IsAssignableTo(typeof(IntValueType)))
        {
            var nativeId = GetIntId(identifier);
            return _mapper.Value.Map(nativeId, idType);
        }
        if (idType == typeof(Guid))
            return GetGuidId(identifier);
        if (idType.IsAssignableTo(typeof(GuidValueType)))
        {
            var nativeId = GetGuidId(identifier);
            return _mapper.Value.Map(nativeId, idType);
        }

        throw new ArgumentOutOfRangeException(nameof(idType), "unknown type");
    }

    public void SaveToFile()
    {
        _dictionary[IncrementIdentifier] = _idIncrement;
        if (_isDirty)
            File.WriteAllText(_fileLocation, JsonSerializer.Serialize(_dictionary));
    }

    public Dictionary<IdIdentifier, object> LoadFromFile()
    {
        if (File.Exists(_fileLocation) is false)
            return new();
        return JsonSerializer.Deserialize<Dictionary<IdIdentifier, object>>(
            File.ReadAllText(_fileLocation)) ?? new();
    }

}