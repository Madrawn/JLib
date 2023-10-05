using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter;
using System;
using System.Collections;
using JLib.Testing;
using Snapshooter.Xunit;
using Xunit.Abstractions;
using static JLib.Tests.Reflection.ServiceCollection.AddDataProvider.AddDataProviderTests;
using FluentAssertions.Common;
using JLib.Tests.Reflection.ServiceCollection.AddDataProvider;

namespace JLib.Tests.Reflection;

public record ReflectionTestOptions(string TestName, string[] ExpectedBehavior, Type[] IncludedTypes,
    Action<IServiceCollection, ITypeCache, IExceptionManager> ServiceFactory, bool TestException = true,
    bool TestCache = true,
    bool TestServices = true)
{
    public ReflectionTestOptions(string TestName, string[] ExpectedBehavior, IEnumerable<Type> IncludedTypes,
        Action<IServiceCollection, ITypeCache, IExceptionManager> ServiceFactory, bool TestException = true,
        bool TestCache = true,
        bool TestServices = true) :
        this(TestName, ExpectedBehavior, IncludedTypes.ToArray(), ServiceFactory, TestException, TestCache, TestServices)
    { }
}

public abstract class ReflectionTestArguments : IEnumerable<object[]>
{
    protected abstract IEnumerable<ReflectionTestOptions> Options { get; }

    public IEnumerator<object[]> GetEnumerator()
        => Options.Select(x => new[] { x as object }).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
public abstract class ReflectionTestBase
{
    private readonly ITestOutputHelper _testOutput;
    private readonly IExceptionManager _exceptions;
    public const string Filter = "";
    public const bool ApplyTestFilter = true;

    protected ReflectionTestBase(ITestOutputHelper testOutput)
    {
        _exceptions = new ExceptionManager(GetType().FullClassName());
        _testOutput = testOutput;
    }
    public virtual void Test(ReflectionTestOptions options)
    {
        _testOutput.WriteLine("TestName: {0}", options.TestName);
        if (!string.IsNullOrWhiteSpace(Filter) && options.TestName != Filter)
        {
            Assert.Fail("skipped");
            return;
        }
        var (testName, expectedBehavior, content, serviceFactory, testExceptions, testCache, testServices) = options;

        IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddTypeCache(
                new[]
                {
                    typeof(ITypeCache).Assembly
                },
                content, out var cache, _exceptions);
        // group by namespace, then by typeValueType and use json objects for the grouping
        var cacheValidator = cache.All<ITypeValueType>()
            .Where(tvt => tvt.Value.Assembly != typeof(ITypeCache).Assembly)
            .PrepareSnapshot();

        services.AddRepositories(cache, _exceptions);
        serviceFactory(services, cache, _exceptions.CreateChild("Service Factory"));

        var exceptionValidator = _exceptions.GetException().PrepareSnapshot();

        var serviceValidator = services.PrepareSnapshot();

        var maxDescriptionLength = expectedBehavior.Max(x => x.Length);
        new Dictionary<string, object?>
        {
            {
                "Parameters",
                new Dictionary<string,object>()
                {
                    {
                        "TestName",
                        testName
                    },
                    {
                        "ExpectedBehavior",
                        expectedBehavior.Select(d=>"  "+d.PadRight(maxDescriptionLength+4))
                    },
                    {
                        "includedTypes",
                        content.Select(c=>c.FullClassName(true))
                    }
                }
            },
            {
                "exception",
                 testExceptions? exceptionValidator:"disabled"
            },
            {
                "cache",
                testCache? cacheValidator:"disabled"
            },
            {
                "services",
                testServices? serviceValidator:"disabled"
            }
        }.MatchSnapshot(new SnapshotNameExtension(testName));
    }
}