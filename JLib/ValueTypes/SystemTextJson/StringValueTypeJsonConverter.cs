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
internal class StringValueTypeJsonConverter<T> : JsonConverter<T>
    where T: ValueType<string>
{
    private readonly IMapper _mapper;

    public StringValueTypeJsonConverter(IMapper mapper)
    {
        _mapper = mapper;
    }
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value is null 
            ? null 
            : _mapper.Map<T>(value);
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