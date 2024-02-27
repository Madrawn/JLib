using System.Text.Json.Serialization;
using JLib.Helper;
using JLib.Reflection;
using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.SystemTextJson;

/// <summary>
/// <see cref="TypeValueType"/> for <see cref="JsonConverter{T}"/> implementations.
/// </summary>
/// <param name="Value"></param>
[NotGeneric, NotAbstract, IsDerivedFrom(typeof(JsonConverter))]
public record JsonConverterType(Type Value) : TypeValueType(Value)
{
    private JsonConverter? _instance;
    /// <summary>
    /// returns a singleton instance of this <see cref="JsonConverterType"/> if it has a parameterless constructor.
    /// </summary>
    /// <returns></returns>
    public JsonConverter? Create(ITypeCache typeCache)
    {
        var ctor = Value.GetConstructor(Array.Empty<Type>())
            ?? Value.GetConstructor(new[] { typeof(ITypeCache) });
        if (ctor is null)
            return null;
        // the only possible parameter is typeCache
        var parameters = ctor.GetParameters().Select(_ => typeCache as object).ToArray();
        return _instance ??= ctor?.Invoke(parameters).CastTo<JsonConverter>();
    }
}