using AutoMapper;
using FluentAssertions;
using JLib.AutoMapper;
using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using JLib.ValueTypes.Mapping.NewtonsoftJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace JLib.ValueTypes.Mapping.Tests;

public class NewtonsoftJsonConverterTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly IMapper _mapper;

    record StringVt(string Value) : StringValueType(Value);
    record GuidVt(Guid Value) : GuidValueType(Value);
    record IntVt(int Value) : IntValueType(Value);

    public NewtonsoftJsonConverterTests(ITestOutputHelper testOutputHelper)
    {
        var loggerFactory = new LoggerFactory().AddXunit(testOutputHelper);
        var exceptions = new ExceptionBuilder("setup");
        var services = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, loggerFactory, JLibReflectionTp.Instance, TypePackage.GetNested<NewtonsoftJsonConverterTests>())
            .AddAutoMapper(m => m.AddProfiles(typeCache, loggerFactory));

        _provider = services.BuildServiceProvider();
        _mapper = _provider.GetRequiredService<IMapper>();

    }

    [Fact]
    public void String_DeserializeObject()
    {
        var value = JsonConvert.DeserializeObject<StringVt>("\"description\"",
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter(_mapper) }
            });
        value.Should().BeOfType<StringVt>();
        value?.Value.Should().BeEquivalentTo("description");
    }
    [Fact]
    public void String_SerializeObject()
    {
        var value = JsonConvert.SerializeObject(new StringVt("description"),
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter(_mapper) }
            });
        value.Should().Be("\"description\"");
    }
    [Fact]
    public void Guid_DeserializeObject()
    {
        var raw = Guid.NewGuid();
        var value = JsonConvert.DeserializeObject<Dictionary<string, GuidVt>>(@$"{{""x"":""{raw}""}}",
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter(_mapper) }
            });
        value.Should().NotBeNull();
        value.Should().ContainKey("x");
        value!.GetValueOrDefault("x").Should().BeOfType<GuidVt>();
        value!["x"].Value.Should().Be(raw);
    }
    [Fact]
    public void Guid_SerializeObject()
    {
        var raw = Guid.NewGuid();
        var value = JsonConvert.SerializeObject(new GuidVt(raw),
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter(_mapper) }
            });
        value?.Should().Be($"\"{raw}\"");
    }
    [Fact]
    public void Int_DeserializeObject()
    {
        var value = JsonConvert.DeserializeObject<Dictionary<string, IntVt>>(@"{""x"":5}",
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter(_mapper) }
            });
        value.Should().NotBeNull();
        value.Should().ContainKey("x");
        value!.GetValueOrDefault("x").Should().BeOfType<IntVt>();
        value!["x"].Value.Should().Be(5);
    }
    [Fact]
    public void Int_SerializeObject()
    {
        var raw = 5;
        var value = JsonConvert.SerializeObject(new IntVt(raw),
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter(_mapper) }
            });
        value.Should().Be(raw.ToString());
    }
#if(DEBUG)
    [Fact (Skip = "niy")]
    public void Dict_SerializeObject()
    {
        var raw = new Dictionary<IntVt, StringVt>()
        {
            {
                new(1),
                new("one")
            }
        };
        var value = JsonConvert.SerializeObject(raw,
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter(_mapper) }
            });
        value.Should().Be("{\"1\":\"one\"}");
    }
    [Fact(Skip = "niy")]
    public void Dict_DeserializeObjectIntKey()
    {
        const string raw = "{1:\"one\"}";
        var value = JsonConvert.DeserializeObject<Dictionary<IntVt, StringVt>>(raw,
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter(_mapper), new ValueTypeDictionaryJsonConverter(_mapper) }
            });
        value.Should().BeOfType<Dictionary<IntVt, StringVt>>();
        value.Should().HaveCount(1);
        value?[new(1)].Value.Should().Be("one");
    }
    [Fact(Skip = "niy")]
    public void Dict_DeserializeObjectStrKey()
    {
        const string raw = "{\"1\":\"one\"}";
        var value = JsonConvert.DeserializeObject<Dictionary<IntVt, StringVt>>(raw,
            new JsonSerializerSettings
            {
                Converters = { new ValueTypeJsonConverter(_mapper), new ValueTypeDictionaryJsonConverter(_mapper) }
            });
        value.Should().BeOfType<Dictionary<IntVt, StringVt>>();
        value.Should().HaveCount(1);
        value?[new(1)].Value.Should().Be("one");
    }
#endif

    public void Dispose()
    {
        _provider.Dispose();
    }
}
