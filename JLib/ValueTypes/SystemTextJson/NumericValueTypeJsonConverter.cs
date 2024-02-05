using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;

namespace JLib.ValueTypes.SystemTextJson;

/// <summary>
/// used by the <see cref="ValueTypeJsonConverterFactory"/> to enable <see cref="ValueType{T}"/> interpretation of types
/// <list type="bullet">
/// <item><see cref="byte"/></item>
/// <item><see cref="sbyte"/></item>
/// <item><see cref="short"/></item>
/// <item><see cref="ushort"/></item>
/// <item><see cref="int"/></item>
/// <item><see cref="uint"/></item>
/// <item><see cref="long"/></item>
/// <item><see cref="ulong"/></item>
/// <item><see cref="decimal"/></item>
/// <item><see cref="double"/></item>
/// <item><see cref="float"/></item>
/// </list>
/// </summary>
internal class NumericValueTypeJsonConverter<T> : JsonConverter<ValueType<T>>
{
    private readonly IMapper _mapper;

    public NumericValueTypeJsonConverter(IMapper mapper)
    {
        _mapper = mapper;
    }
    public override ValueType<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        object value = typeof(T) switch
        {
            { } type when type == typeof(byte) => reader.GetByte(),
            { } type when type == typeof(sbyte) => reader.GetSByte(),
            { } type when type == typeof(short) => reader.GetUInt16(),
            { } type when type == typeof(ushort) => reader.GetInt16(),
            { } type when type == typeof(int) => reader.GetInt32(),
            { } type when type == typeof(uint) => reader.GetUInt32(),
            { } type when type == typeof(long) => reader.GetInt64(),
            { } type when type == typeof(ulong) => reader.GetUInt64(),
            { } type when type == typeof(decimal) => reader.GetDecimal(),
            { } type when type == typeof(double) => reader.GetDouble(),
            { } type when type == typeof(float) => reader.GetSingle(),
            _ => throw new NotSupportedException(
                $"Type {typeof(T).FullName} is not supported. Only Numbers are supported")
        };
        return (ValueType<T>)_mapper.Map(value, value.GetType(), typeToConvert);
    }

    public override void Write(Utf8JsonWriter writer, ValueType<T>? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        switch (value.Value)// required to select the correct overload
        {
            case byte v:
                writer.WriteNumberValue(v);
                break;
            case sbyte v:
                writer.WriteNumberValue(v);
                break;
            case short v:
                writer.WriteNumberValue(v);
                break;
            case ushort v:
                writer.WriteNumberValue(v);
                break;
            case int v:
                writer.WriteNumberValue(v);
                break;
            case uint v:
                writer.WriteNumberValue(v);
                break;
            case long v:
                writer.WriteNumberValue(v);
                break;
            case ulong v:
                writer.WriteNumberValue(v);
                break;
            case decimal v:
                writer.WriteNumberValue(v);
                break;
            case double v:
                writer.WriteNumberValue(v);
                break;
            case float v:
                writer.WriteNumberValue(v);
                break;
        }

    }
}