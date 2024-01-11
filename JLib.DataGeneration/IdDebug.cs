using System.Diagnostics;
using System.Text.Json;
using JLib.DataGeneration;
using JLib.Helper;
using JLib.ValueTypes;

/// <summary>
/// adds debug methods to resolve the name of an id managed by a <see cref="IIdRegistry"/>
/// </summary>
// ReSharper disable once CheckNamespace
public static class IdDebug
{
    // there might be more than one instance if the tests are executed in parallel. If that's the case, there are no new keys expected.
    private static readonly object InstancesLock = new();
    private static readonly List<IReadOnlyDictionary<DataPackageValues.IdIdentifier, object>> Instances = new();
#if(DEBUG)
    public record IdInformation(Type Type, DataPackageValues.IdIdentifier Identifier, object Value)
    {
        public override string ToString() => Type.FullClassName() + " " + Identifier + " = " + Value;
    }

    /// <summary>
    /// searches for the given id across all IdRegistry Instances and returns all associated keys
    /// </summary>
    public static string? GetIdInfo(object? value)
    {
        if (value is null)
            return null;
        var nativeValue = value switch
        {
            IntValueType intVt => intVt.Value,
            StringValueType stringVt => stringVt.Value,
            GuidValueType guidVt => guidVt.Value,
            _ => value
        };
        lock (InstancesLock)
        {
            var values = Instances.SelectMany(dict => dict)
                .Where(kv => kv.Value.Equals(nativeValue))
                .Select(kv => $"{kv.Key.IdGroupName.Value}.{kv.Key.IdName.Value}")
                .Distinct()
                .ToArray();

            return values.Length switch
            {
                0 => "not found",
                1 => value.GetType().FullClassName() + " " + values.Single() + " = " + nativeValue,
                _ => JsonSerializer.Serialize(values)
            };
        }
    }
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/> as structured data
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static IdInformation? GetIdInfoObj(object? value)
    {
        if (value is null)
            return null;
        var nativeValue = value switch
        {
            IntValueType intVt => intVt.Value,
            StringValueType stringVt => stringVt.Value,
            GuidValueType guidVt => guidVt.Value,
            _ => value
        };
        lock (InstancesLock)
        {
            return Instances.SelectMany(dict => dict)
                .Where(kv => kv.Value.Equals(nativeValue))
                .Select(kv => new IdInformation(value.GetType(), kv.Key, value))
                .Distinct()
                .Single();
        }
    }

    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? Info(this Guid? id) => GetIdInfo(id);

    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? Info(this Guid id) => GetIdInfo(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? Info(this GuidValueType id) => GetIdInfo(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? Info(this int? id) => GetIdInfo(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? Info(this int id) => GetIdInfo(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? Info(this IntValueType id) => GetIdInfo(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? IdInfo(this string? id) => GetIdInfo(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? Info(this StringValueType id) => GetIdInfo(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? GuidInfo(this string? id) => id is null
        ? null
        : Guid.TryParse(id, out var guid)
        ? GetIdInfo(guid)
        : throw new FormatException($"'{id}' is not a guid");

    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this Guid id) => GetIdInfoObj(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this Guid? id) => GetIdInfoObj(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this GuidValueType id) => GetIdInfoObj(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this int id) => GetIdInfoObj(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this int? id) => GetIdInfoObj(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this IntValueType id) => GetIdInfoObj(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? IdInfoObj(this string id) => GetIdInfoObj(id);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this StringValueType id) => GetIdInfoObj(id);
#endif
    internal static void Register(IReadOnlyDictionary<DataPackageValues.IdIdentifier, object> keys)
    {
        lock (InstancesLock)
            Instances.Add(keys);
    }

    internal static void UnRegister(IReadOnlyDictionary<DataPackageValues.IdIdentifier, object> keys)
    {
        lock (InstancesLock)
            Instances.Remove(keys);
    }
}