using System.Runtime.CompilerServices;
using System.Text.Json;
using JLib.Helper;
using static JLib.DataGeneration.DataPackageValues;

namespace JLib.DataGeneration;

internal class IdRegistry
{


    private readonly string _fileLocation;
    private readonly Dictionary<IdIdentifier, object> _dictionary;
    private bool _isDirty;
    // todo: save & load
    private int _idIncrement = 1;
    public IdRegistry(string? fileLocation = null)
    {
        _fileLocation = fileLocation ?? GetFileName();
        _dictionary = LoadFromFile();
    }

    private string GetFileName()
    {
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        var targetDir = Parent(currentDir);
        var file = Path.Combine(targetDir, "DataPackageStore.json");
        return file;

        string Parent(string dir)
        {
            return Directory.GetFiles(dir, "*.csproj").Any()
                    ? dir
                    : Parent(Directory.GetParent(dir)?.FullName!);
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

    public void SaveToFile()
    {
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