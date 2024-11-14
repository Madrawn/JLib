using System.Text.Encodings.Web;
using System.Text.Json;
using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.Reflection;

/// <summary>
/// Extension methods for <see cref="ITypePackage"/>
/// </summary>
public static class TypePackageExtensions
{
    /// <summary>
    /// applies the given <paramref name="filter"/> to the typePackage <paramref name="typePackage"/>
    /// </summary>
    public static ITypePackage ApplyFilter(this ITypePackage typePackage, Func<Type, bool> filter, string? filterDescription = null)
        => TypePackage.Get(
            typePackage.Types.Where(filter),
            typePackage.Children.Select(c => c.ApplyFilter(filter, filterDescription)),
            typePackage.DescriptionTemplate + (filterDescription is null ? "" : $"Filtered: {filterDescription}"));

    /// <summary>
    /// Creates a <see cref="ITypePackage"/> from the given <paramref name="typePackage"/> which contains only <see cref="TypeValueType"/>s and <see cref="ITypeValueType"/>s
    /// </summary>
    public static ITypePackage RemoveNonTypeValueTypes(this ITypePackage typePackage)
        => typePackage.ApplyFilter(
            type => type.IsDerivedFromAny<ITypeValueType>() || type.IsDerivedFromAny<TypeValueType>(),
            nameof(RemoveNonTypeValueTypes)
        );
    /// <summary>
    /// Creates a <see cref="ITypePackage"/> from the given <paramref name="typePackage"/> which contains only <see cref="ValueTypes.ValueType"/>s
    /// </summary>
    public static ITypePackage RemoveNonValueTypes(this ITypePackage typePackage)
        => typePackage.ApplyFilter(
            type => type.IsDerivedFromAny<ValueType<Ignored>>(),
            nameof(RemoveNonValueTypes)
        );


    private static readonly JsonSerializerOptions DefaultOptions =
        new JsonSerializerOptions(JsonSerializerDefaults.General)
        { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    /// <summary>
    /// returns a json representation of the given <paramref name="typePackage"/>
    /// </summary>
    public static string ToJson(this ITypePackage typePackage, JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(typePackage.ToJsonObject(), options ?? DefaultOptions);

    /// <summary>
    /// returns a json optimized object representation of the given <paramref name="typePackage"/>
    /// </summary>
    public static object ToJsonObject(this ITypePackage typePackage)
    {
        var res = new Dictionary<string, object?>()
        {
            ["Description"] = typePackage.DescriptionTemplate
                .Replace("{Children}", typePackage.Children.Count().ToString())
                .Replace("{Types}", typePackage.Types.Count().ToString()),
            ["Types"] = typePackage.Types.GroupBy(t => t.Namespace)
                .ToDictionary(
                    kv => kv.Key ?? "-",
                    kv => kv.Select(t => t.FullName()).ToReadOnlyCollection()
                ),
            ["Children"] = typePackage.Children.Select(ToJsonObject).ToReadOnlyCollection()
        };
        res.RemoveWhere(x => x.Value is null);
        return res;
    }
}