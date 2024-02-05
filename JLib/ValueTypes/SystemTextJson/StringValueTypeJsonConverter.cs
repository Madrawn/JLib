using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;

namespace JLib.ValueTypes.SystemTextJson;

/// <summary>
/// used by the <see cref="ValueTypeJsonConverterFactory"/> to enable <see cref="ValueType{T}"/> interpretation of types
/// <list type="bullet">
/// <item><see cref="string"/></item>
/// </list>
/// </summary>
internal class StringValueTypeJsonConverter : JsonConverter<ValueType<string>>
{
    private readonly IMapper _mapper;

    public StringValueTypeJsonConverter(IMapper mapper)
    {
        _mapper = mapper;
    }
    public override ValueType<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null)
            return null;
        return (ValueType<string>)_mapper.Map(value, value.GetType(), typeToConvert);
    }

    public override void Write(Utf8JsonWriter writer, ValueType<string>? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value);
    }
}