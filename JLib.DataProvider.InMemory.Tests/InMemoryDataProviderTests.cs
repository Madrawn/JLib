﻿using FluentAssertions;
using JLib.Exceptions;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace JLib.DataProvider.InMemory.Tests;
public class InMemoryDataProviderTests
{
    private readonly IDataProviderRw<TestEntity> _dataProvider;

    [TvtFactoryAttribute.Implements(typeof(ITestEntity)), TvtFactoryAttribute.Priority(NextPriority - 1000)]
    public record TestEntityType(Type Value) : EntityType(Value);
    public interface ITestEntity : IEntity
    {

    }
    public class TestEntity : ITestEntity
    {
        public Guid Id { get; set; }

    }
    public InMemoryDataProviderTests(ITestOutputHelper testOutputHelper)
    {
        var loggerFactory = new LoggerFactory()
            .AddXunit(testOutputHelper);
        ExceptionBuilder exceptions = new ExceptionBuilder("Test");
        IServiceCollection services = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, loggerFactory,
                TypePackage.GetNested<InMemoryDataProviderTests>())
            .AddDataProvider<TestEntityType, InMemoryDataProvider<ITestEntity>, ITestEntity>(
                typeCache, null, null, null, exceptions, loggerFactory)
            .AddLogging(c=>c.AddXunit(testOutputHelper));
        ;
        exceptions.ThrowIfNotEmpty();
        var provider = services.BuildServiceProvider();
        _dataProvider = provider.GetRequiredService<IDataProviderRw<TestEntity>>();
    }

    [Fact]
    public void AddSingle()
    {
        var e = new TestEntity();
        _dataProvider.Add(e);
        _dataProvider.Get().Should().ContainSingle(t => t == e);
    }
}
