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
    private static readonly List<IIdRegistry> Instances = new();

    public record IdInformation(Type Type, DataPackageValues.IdIdentifier Identifier, object Value)
    {
        public override string ToString() => Type.FullClassName() + " " + Identifier + " = " + Value;
    }

    /// <summary>
    /// searches for the given id across all IdRegistry Instances and returns all associated keys
    /// </summary>
    public static string? GetIdInfo(object? value, IIdRegistry? idRegistry = null)
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
        if (idRegistry is not null)
            return GetInfoString(idRegistry.GetIdentifierOfId(value));

        lock (InstancesLock)
        {
            var values = Instances.Select(dict => dict.GetIdentifierOfId(value))
                .WhereNotNull()
                .Select(GetInfoString)
                .Distinct()
                .ToArray();

            return values.Length switch
            {
                0 => "not found",
                1 => values.Single(),
                _ => JsonSerializer.Serialize(values)
            };
        }

        string GetInfoString(DataPackageValues.IdIdentifier? identifier) =>
            $"{value?.GetType().FullClassName()} {identifier} = {nativeValue}";
    }
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/> as structured data
    /// </summary>
    public static IdInformation? GetIdInfoObj(object? value, IIdRegistry? idRegistry = null)
    {
        if (value is null)
            return null;

        if (idRegistry is null)
        {
            lock (InstancesLock)
                return Instances.Select(dict => dict.GetIdentifierOfId(value))
                    .WhereNotNull()
                    .Select(identifier => new IdInformation(value.GetType(), identifier, value))
                    .Distinct()
                    .Single();
        }

        var identifier = idRegistry.GetIdentifierOfId(value);
        return identifier is null
            ? null
            : new IdInformation(value.GetType(), identifier, value);

    }

    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? IdInfo(this Guid? id, IIdRegistry? idRegistry = null) => GetIdInfo(id, idRegistry);

    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? IdInfo(this Guid id, IIdRegistry? idRegistry = null) => GetIdInfo(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? IdInfo(this GuidValueType id, IIdRegistry? idRegistry = null) => GetIdInfo(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? IdInfo(this int? id, IIdRegistry? idRegistry = null) => GetIdInfo(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? IdInfo(this int id, IIdRegistry? idRegistry = null) => GetIdInfo(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? IdInfo(this IntValueType id, IIdRegistry? idRegistry = null) => GetIdInfo(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? IdInfo(this string? id, IIdRegistry? idRegistry = null) => GetIdInfo(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfo"/>
    /// </summary>
    public static string? IdInfo(this StringValueType id, IIdRegistry? idRegistry = null) => GetIdInfo(id, idRegistry);
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
    public static IdInformation? InfoObj(this Guid id, IIdRegistry? idRegistry = null) => GetIdInfoObj(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this Guid? id, IIdRegistry? idRegistry = null) => GetIdInfoObj(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this GuidValueType id, IIdRegistry? idRegistry = null) => GetIdInfoObj(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this int id, IIdRegistry? idRegistry = null) => GetIdInfoObj(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this int? id, IIdRegistry? idRegistry = null) => GetIdInfoObj(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this IntValueType id, IIdRegistry? idRegistry = null) => GetIdInfoObj(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? IdInfoObj(this string id, IIdRegistry? idRegistry = null) => GetIdInfoObj(id, idRegistry);
    /// <summary>
    /// <inheritdoc cref="GetIdInfoObj"/>
    /// </summary>
    public static IdInformation? InfoObj(this StringValueType id, IIdRegistry? idRegistry = null) => GetIdInfoObj(id, idRegistry);

    internal static void Register(IIdRegistry idRegistry)
    {
        lock (InstancesLock)
            Instances.Add(idRegistry);
    }

    internal static void UnRegister(IIdRegistry idRegistry)
    {
        lock (InstancesLock)
            Instances.Remove(idRegistry);
    }
}