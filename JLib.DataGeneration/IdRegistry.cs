using JLib.Helper;
using Newtonsoft.Json;

namespace JLib.DataGeneration;

internal class IdRegistry
{
    private readonly string _fileLocation;
    private readonly Dictionary<string, Dictionary<string, Guid>> _dictionary;
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
    public Guid GetId(string group, string value)
        => _dictionary.GetValueOrAdd(group, () => new()).GetValueOrAdd(value, Guid.NewGuid);

    public void SaveToFile()
        => File.WriteAllText(_fileLocation, JsonConvert.SerializeObject(_dictionary, Formatting.Indented));

    public Dictionary<string, Dictionary<string, Guid>> LoadFromFile()
    {
        if (File.Exists(_fileLocation) is false)
            return new();
        return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Guid>>>(File.ReadAllText(_fileLocation)) ?? new();
    }

}