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
internal class GuidValueTypeJsonConverter : JsonConverter<ValueType<Guid>?>
{
    private readonly IMapper _mapper;

    public GuidValueTypeJsonConverter(IMapper mapper)
    {
        _mapper = mapper;
    }
    public override ValueType<Guid>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null)
            return null;
        var guid = Guid.Parse(value);
        return (ValueType<Guid>)_mapper.Map(guid, guid.GetType(), typeToConvert);
    }

    public override void Write(Utf8JsonWriter writer, ValueType<Guid>? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStringValue(value.Value);
    }
}