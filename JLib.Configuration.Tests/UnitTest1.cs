using FluentAssertions;
using JLib.Exceptions;
using JLib.Reflection.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace JLib.Configuration.Tests;

public class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    [ConfigSectionName("Demo")]
    public class DemoConfig
    {
        public string? ConfigProperty { get; init; }
    }

    private void TestBase(Action<DemoConfig> validator, params Dictionary<string, string?>[] configValues)
    {
        using var exceptions = new ExceptionBuilder(nameof(Test1));
        var logger = new LoggerFactory().AddXunit(_testOutputHelper);
        var configBuilder = new ConfigurationBuilder();

        foreach (var value in configValues)
            configBuilder.Sources.Add(new MemoryConfigurationSource()
            {
                InitialData = value
            });

        var config = configBuilder.Build();
        var services = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, logger, "JLib.")
            .AddAllConfigSections(typeCache, config, logger, ServiceLifetime.Singleton);

        using var serviceProvider = services.BuildServiceProvider();

        validator(serviceProvider.GetRequiredService<DemoConfig>());
    }

    [Fact]
    public void Test1()
    {
        TestBase(cfg =>
        {
            cfg.ConfigProperty.Should().Be("value1");
        }, new Dictionary<string, string?>()
        {
            {
                "Demo:ConfigProperty", "value1"
            }
        });
    }
    [Fact]
    public void Test2()
    {
        TestBase(cfg =>
        {
            cfg.ConfigProperty.Should().Be("value1");
        }, new Dictionary<string, string?>()
        {
            {
                "Environment", "Development"
            }
        }, new Dictionary<string, string?>()
        {
            {
                "Environment", null
            },
            {
                "Demo:ConfigProperty", "value1"
            }
        }
            );
    }
    [Fact]
    public void Test3()
    {
        TestBase(cfg =>
        {
            cfg.ConfigProperty.Should().Be("value2");
        }, new Dictionary<string, string?>()
        {
            {
                "Environment", "dev"
            },
            {
                "Demo:ConfigProperty", "value1"
            },
            {
                "Demo:dev:ConfigProperty", "value2"
            }
        });
    }
}
