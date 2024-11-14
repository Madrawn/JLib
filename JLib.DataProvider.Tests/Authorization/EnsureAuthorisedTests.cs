using FluentAssertions;
using JLib.Cqrs;
using JLib.DataProvider.Authorization;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataProvider.Tests.Authorization;

public class EnsureAuthorisedTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public EnsureAuthorisedTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void PositiveTest()
    {
        using var loggerFactory = new LoggerFactory().AddXunit(_testOutputHelper);

        var exceptions = new ExceptionBuilder("test");

        var cache = new TypeCache(
            TypePackage.Get(
                JLibDataProviderTp.Instance,
                JLibCqrsTp.Instance,
                TypePackage.Get(typeof(TestDataObject), typeof(ValidAuthProfile))
            ),
            exceptions, loggerFactory);

        exceptions.GetException()?.Message.Should().BeNull();
    }
    [Fact]
    public void NegativeTest()
    {
        using var loggerFactory = new LoggerFactory().AddXunit(_testOutputHelper);

        var exceptions = new ExceptionBuilder("test");

        var cache = new TypeCache(
            TypePackage.Get(
                JLibDataProviderTp.Instance,
                JLibCqrsTp.Instance,
                TypePackage.Get(typeof(TestDataObject), typeof(InvalidAuthProfile))
                    ),
            exceptions, loggerFactory);

        var sut = () => exceptions.ThrowIfNotEmpty();
        sut.Should().Throw<JLibAggregateException>()
            .Where(x => x.FlattenAll().OfType<UnknownDataObjectTypeException>().Any() == false,
                "test setup misconfigured")
            .Where(x => x.FlattenAll().OfType<MissingAuthorizationException>().Count() == 1,
                "missing entity detected")
            .Where(x => x.FlattenAll().OfType<NotUniqueTypeFilterException<CommandEntityType>>().Any() == false);

    }


    public class InvalidAuthProfile : AuthorizationProfile
    {
        public InvalidAuthProfile(ITypeCache typeCache) : base(typeCache)
        {
            EnsureAuthorised<CommandEntityType>().ThrowIfNotEmpty();
        }
    }
    public class ValidAuthProfile : AuthorizationProfile
    {
        public ValidAuthProfile(ITypeCache typeCache) : base(typeCache)
        {
            AddAuthorization<TestDataObject, Ignored>((_, _) => true);
            var ex = EnsureAuthorised<CommandEntityType>();
        }
    }
    public class TestDataObject : ICommandEntity
    {
        public Guid Id { get; init; }
    }
}