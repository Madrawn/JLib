using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using AutoMapper;
using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static JLib.DataGeneration.DataPackageValues;
using JsonSerializer = System.Text.Json.JsonSerializer;
using SaveFileType = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, System.Text.Json.JsonElement>>;

namespace JLib.DataGeneration;

internal enum DataPackageInitState
{
    Uninitialized,
    Initializing,
    Initialized
}


public interface IIdRegistry
{
    public string GetStringId(IdIdentifier identifier);
    public Guid GetGuidId(IdIdentifier identifier);
    public int GetIntId(IdIdentifier identifier);
    public IdIdentifier? GetIdentifierOfId(object? id);
    public IdIdentifier? GetIdentifierOfId(string id);
    public IdIdentifier? GetIdentifierOfId(StringValueType id);
    public IdIdentifier? GetIdentifierOfId(int id);
    public IdIdentifier? GetIdentifierOfId(int? id);
    public IdIdentifier? GetIdentifierOfId(IntValueType id);
    public IdIdentifier? GetIdentifierOfId(Guid id);
    public IdIdentifier? GetIdentifierOfId(Guid? id);
    public IdIdentifier? GetIdentifierOfId(GuidValueType id);
    internal void SetIdPropertyValue(object packageInstance, PropertyInfo property);
    internal void SaveToFile();
}

internal class IdRegistry : IIdRegistry, IDisposable
{

    private static readonly IdIdentifier IncrementIdentifier = new(new("__registry__"), new("IdIncrement"));

    private readonly Lazy<IMapper> _mapper;
    private readonly string _fileLocation;
    private readonly ConcurrentDictionary<IdIdentifier, object> _dictionary;
    private bool _isDirty;
    private int _idIncrement;
    public IdRegistry(IServiceProvider serviceProvider)
    {
        _fileLocation = GetFileName();
        _dictionary = LoadFromFile().ToConcurrentDictionary();
        _idIncrement = _dictionary.GetValueOrDefault(IncrementIdentifier) as int? ?? 0;
        _dictionary.Remove(IncrementIdentifier, out _);
        _mapper = new(serviceProvider.GetRequiredService<IMapper>);
        IdDebug.Register(this);
    }

    private static string GetFileName()
    {
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        var targetDir = Parent(currentDir);
        var file = Path.Combine(targetDir, "IdRegistry.json");
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
    public string GetStringId(IdIdentifier identifier)
    {
        _isDirty = true;
        return _dictionary.GetValueOrAdd(identifier, () =>
        {
            _isDirty = true;
            return $"{identifier.IdGroupName.Value}.{identifier.IdName.Value}:{Interlocked.Increment(ref _idIncrement)}";
        }).CastTo<string>();
    }

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
            return Interlocked.Increment(ref _idIncrement);
        }).CastTo<int>();

    public IdIdentifier? GetIdentifierOfId(object? id)
    {
        var nativeId = id switch
        {
            IntValueType intVt => intVt.Value,
            StringValueType stringVt => stringVt.Value,
            GuidValueType guidVt => guidVt.Value,
            _ => id
        };
        return _dictionary.Single(kv => kv.Value.Equals(nativeId)).Key;
    }

    public IdIdentifier? GetIdentifierOfId(string id)
        => GetIdentifierOfId((object)id);
    public IdIdentifier? GetIdentifierOfId(StringValueType id)
        => GetIdentifierOfId((object)id.Value);
    public IdIdentifier? GetIdentifierOfId(int id)
        => GetIdentifierOfId((object)id);
    public IdIdentifier? GetIdentifierOfId(int? id)
        => GetIdentifierOfId((object?)id);
    public IdIdentifier? GetIdentifierOfId(IntValueType id)
        => GetIdentifierOfId((object)id.Value);
    public IdIdentifier? GetIdentifierOfId(Guid id)
        => GetIdentifierOfId((object)id);
    public IdIdentifier? GetIdentifierOfId(Guid? id)
        => GetIdentifierOfId((object?)id);
    public IdIdentifier? GetIdentifierOfId(GuidValueType id)
        => GetIdentifierOfId((object)id.Value);

    /// <summary>
    /// sets the id of the given <paramref name="property"/> to the persisted id or creates a new one<br/>
    /// throws a <see cref="ArgumentOutOfRangeException"/> when the property is neither a <see cref="int"/>, <see cref="Guid"/>, <see cref="IntValueType"/> nor <see cref="GuidValueType"/>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    void IIdRegistry.SetIdPropertyValue(object packageInstance, PropertyInfo property)
    {
        var packageType = property.ReflectedType
                          ?? throw new Exception("Property has no Reflected type");

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
            return _mapper.Value.Map(nativeId, nativeId.GetType(), idType);
        }
        if (idType == typeof(Guid))
            return GetGuidId(identifier);
        if (idType.IsAssignableTo(typeof(GuidValueType)))
        {
            var nativeId = GetGuidId(identifier);
            return _mapper.Value.Map(nativeId, nativeId.GetType(), idType);
        }
        if (idType == typeof(string))
            return GetStringId(identifier);
        if (idType.IsAssignableTo(typeof(StringValueType)))
        {
            var nativeId = GetStringId(identifier);
            return _mapper.Value.Map(nativeId, nativeId.GetType(), idType);
        }

        throw new ArgumentOutOfRangeException(nameof(idType), "unknown type");
    }

    public void SaveToFile()
    {
        if (!_isDirty)
            return;

        var obj = _dictionary
            .Append(new(IncrementIdentifier, _idIncrement))
            .GroupBy(kv => kv.Key.IdGroupName.Value)
            .ToImmutableSortedDictionary(g => g.Key,
                g => g.ToImmutableSortedDictionary(kv => kv.Key.IdName.Value,
                    kv => kv.Value));
        var str = JsonSerializer.Serialize(obj, new JsonSerializerOptions()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        File.WriteAllText(_fileLocation, str);
    }


    private object DeserializeId(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Number:
                return value.GetInt32();
            case JsonValueKind.String:
                {
                    var raw = value.GetString() ?? throw new InvalidSetupException("read empty id from file");
                    return Guid.TryParse(raw, out var guid)
                        ? guid
                        : raw;
                }
            default:
                throw new IndexOutOfRangeException(nameof(value.ValueKind));
        }
    }
    public Dictionary<IdIdentifier, object> LoadFromFile()
    {
        if (File.Exists(_fileLocation) is false)
            return new();
        var str = File.ReadAllText(_fileLocation);
        var raw1 = JsonSerializer.Deserialize<SaveFileType>(str) ?? new();
        var raw2 = raw1
            .SelectMany(groupName => groupName.Value
            .ToDictionary(
                name => new IdIdentifier(new(groupName.Key), new(name.Key)),
                x => DeserializeId(x.Value)
            ))
            .ToDictionary(x => x.Key, x => x.Value);
        return raw2;
    }

    public void Dispose()
    {
        // otherwise at runtime generated ids won't be persisted
        SaveToFile();
        IdDebug.UnRegister(this);
    }
}