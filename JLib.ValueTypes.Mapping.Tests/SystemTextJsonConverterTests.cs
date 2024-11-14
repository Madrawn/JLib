﻿using System.Text.Json;
using AutoMapper;
using FluentAssertions;
using JLib.AutoMapper;
using JLib.Exceptions;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using JLib.ValueTypes.Mapping.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace JLib.ValueTypes.Mapping.Tests;

public class SystemTextJsonConverterTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IMapper _mapper;

    record StringVt(string Value) : StringValueType(Value);
    record GuidVt(Guid Value) : GuidValueType(Value);
    record IntVt(int Value) : IntValueType(Value);

    public SystemTextJsonConverterTests(ITestOutputHelper testOutputHelper)
    {
        var loggerFactory = new LoggerFactory().AddXunit(testOutputHelper);
        var exceptions = new ExceptionBuilder("setup");
        var services = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, loggerFactory, JLibReflectionTp.Instance, TypePackage.GetNested<SystemTextJsonConverterTests>())
            .AddAutoMapper(m => m.AddProfiles(typeCache, loggerFactory));

        _provider = services.BuildServiceProvider();
        _mapper = _provider.GetRequiredService<IMapper>();

    }

    [Fact]
    public void String_Deserialize()
    {
        var value = JsonSerializer.Deserialize<StringVt>("\"description\"",
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory(_mapper) }
            });
        value.Should().BeOfType<StringVt>();
        value?.Value.Should().BeEquivalentTo("description");
    }
    [Fact]
    public void String_Serialize()
    {
        var value = JsonSerializer.Serialize(new StringVt("description"),
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory(_mapper) }
            });
        value.Should().Be("\"description\"");
    }
    [Fact]
    public void Guid_Deserialize()
    {
        var raw = Guid.NewGuid();
        var value = JsonSerializer.Deserialize<GuidVt>(raw,
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory(_mapper) }
            });
        value.Should().BeOfType<GuidVt>();
        value?.Value.Should().Be(raw);
    }
    [Fact]
    public void Guid_Serialize()
    {
        var raw = Guid.NewGuid();
        var value = JsonSerializer.Serialize(new GuidVt(raw),
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory(_mapper) }
            });
        value?.Should().Be($"\"{raw}\"");
    }
    [Fact]
    public void Int_Deserialize()
    {
        var raw = 5;
        var value = JsonSerializer.Deserialize<IntVt>(raw,
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory(_mapper) }
            });
        value.Should().BeOfType<IntVt>();
        value?.Value.Should().Be(raw);
    }
    [Fact]
    public void Int_Serialize()
    {
        var raw = 5;
        var value = JsonSerializer.Serialize(new IntVt(raw),
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory(_mapper) }
            });
        value.Should().Be(raw.ToString());
    }
    [Fact]
    public void Dict_Serialize()
    {
        var raw = new Dictionary<IntVt, StringVt>()
        {
            {
                new(1),
                new("one")
            }
        };
        var value = JsonSerializer.Serialize(raw,
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory(_mapper) }
            });
        value.Should().Be("{\"1\":\"one\"}");
    }
    [Fact]
    public void Dict_Deserialize()
    {
        const string raw = "{\"1\":\"one\"}";
        var value = JsonSerializer.Deserialize<Dictionary<IntVt,StringVt>>(raw,
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new ValueTypeJsonConverterFactory(_mapper) }
            });
        value.Should().BeOfType<Dictionary<IntVt, StringVt>>();
        value.Should().HaveCount(1);
        value?[new(1)].Value.Should().Be("one");
    }


    public void Dispose()
    {
        _provider.Dispose();
    }
}
