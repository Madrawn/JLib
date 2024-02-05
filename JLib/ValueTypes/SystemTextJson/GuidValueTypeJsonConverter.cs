using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;

namespace JLib.ValueTypes.SystemTextJson;

/// <summary>
/// used by the <see cref="ValueTypeJsonConverterFactory"/> to enable <see cref="ValueType{T}"/> interpretation of types
/// <list type="bullet">
/// <item><see cref="Guid"/></item>
/// </list>
/// </summary>
internal class GuidValueTypeJsonConverter<T> : JsonConverter<T>
    where T : ValueType<Guid>?
{
    private readonly IMapper _mapper;

    public GuidValueTypeJsonConverter(IMapper mapper)
    {
        _mapper = mapper;
    }
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null)
            return null;
        var guid = Guid.Parse(value);
        return (T)_mapper.Map(guid, guid.GetType(), typeToConvert);
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStringValue(value.Value);
    }
}