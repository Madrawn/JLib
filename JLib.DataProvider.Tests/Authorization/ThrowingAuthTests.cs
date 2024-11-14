using FluentAssertions;
using JLib.DataProvider.Authorization;
using JLib.Exceptions;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataProvider.Tests.Authorization;

public class ThrowingAuthTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ThrowingAuthTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    [Fact]
    public void Test()
    {
        using var loggerFactory = new LoggerFactory().AddXunit(_testOutputHelper);

        var exceptions = new ExceptionBuilder("test");

        new ServiceCollection()
            .AddTypeCache(
                out var typeCache, exceptions, loggerFactory,
                JLibDataProviderTp.Instance,
                TypePackage.GetNested<ThrowingAuthTests>()
            );
        exceptions.GetException().Should().NotBeNull();
    }
    public class ThrowingTestAuthProfile : AuthorizationProfile
    {
        public ThrowingTestAuthProfile(ITypeCache typeCache) : base(typeCache)
        {
            throw new CustomTestException();
        }
    }

    public class CustomTestException : Exception { }
}