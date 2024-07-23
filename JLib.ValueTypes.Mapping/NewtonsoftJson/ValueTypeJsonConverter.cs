using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using AutoMapper;
using System.Formats.Asn1;
using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes.Mapping.SystemTextJson;

// Other necessary using directives...

namespace JLib.ValueTypes.Mapping.NewtonsoftJson;
// not implemented yet
#if DEBUG
public class ValueTypeDictionaryJsonConverter : JsonConverter
{
    private readonly IMapper _mapper;

    public ValueTypeDictionaryJsonConverter(IMapper mapper)
    {
        _mapper = mapper;
    }
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var keyType = objectType.GenericTypeArguments.First();
        var nativeKeyType = objectType.GetAnyBaseType<ValueType<Ignored>>()?.GenericTypeArguments.Single();
        var nativeDictType = typeof(Dictionary<,>).MakeGenericType(typeof(string), objectType.GenericTypeArguments[1]);
        var nativeDict = Activator.CreateInstance(nativeDictType)
            ?? throw new InvalidOperationException("dictionary could not be created");

        serializer.Populate(reader, nativeDict);

        return _mapper.Map(nativeDict, nativeDictType, objectType);
    }

    public override bool CanConvert(Type objectType)
        => objectType.IsGenericTypeDefinition
           && objectType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
           && objectType.GetGenericTypeDefinition().GenericTypeArguments.First() == typeof(ValueType<>);
}
#endif
public class ValueTypeJsonConverter : JsonConverter
{
    private IMapper? _mapper;
    private readonly ConcurrentDictionary<Type, JsonConverter> _converters = new();

    public ValueTypeJsonConverter(IMapper? mapper)
    {
        _mapper = mapper;
    }

    public void AddMapper(IMapper mapper)
    {
        if (_mapper is not null)
            throw new InvalidOperationException("Mapper has already been set");
        _mapper = mapper;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType.IsDerivedFromAny<ValueType<Ignored>>();
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (_mapper is null)
            throw new InvalidOperationException($"Mapper has not been set. Use {AddMapper} to set the AutoMapper reference");

        // todo: add dictionary primary key support
        var nativeType = objectType
            .GetAnyBaseType<ValueType<Ignored>>()
            ?.GenericTypeArguments.First();
        object? value = nativeType
            switch
        {
            not null when reader.Value == null => null,
            not null when nativeType == typeof(Guid) => Guid.Parse((string)reader.Value),
            not null when nativeType == typeof(string) => reader.Value,
            not null when nativeType == typeof(byte) => Convert.ToByte(reader.Value),
            not null when nativeType == typeof(sbyte) => Convert.ToSByte(reader.Value),
            not null when nativeType == typeof(short) => Convert.ToInt16(reader.Value),
            not null when nativeType == typeof(ushort) => Convert.ToUInt16(reader.Value),
            not null when nativeType == typeof(int) => Convert.ToInt32(reader.Value),
            not null when nativeType == typeof(uint) => Convert.ToUInt32(reader.Value),
            not null when nativeType == typeof(long) => Convert.ToInt64(reader.Value),
            not null when nativeType == typeof(ulong) => Convert.ToUInt64(reader.Value),
            not null when nativeType == typeof(decimal) => Convert.ToDecimal(reader.Value),
            not null when nativeType == typeof(double) => Convert.ToDouble(reader.Value),
            not null when nativeType == typeof(float) => Convert.ToSingle(reader.Value),
            _ => throw new NotSupportedException(
                $"Type {objectType.FullName()} is not supported. Only Numbers are supported")
        };
        return _mapper.Map(value, nativeType, objectType);

    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value?.GetType().IsDerivedFromAny<ValueType<Ignored>>() == false)
            throw new NotSupportedException($"Type {value.GetType().FullName()} is not supported");

        writer.WriteValue(value?.GetType().GetProperty(nameof(ValueType<Ignored>.Value))?.GetValue(value));
    }

}