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
using Serilog;

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
    protected virtual string Filter { get; } = "";
    protected virtual bool ListSkippedTests { get; } = false;

    public IEnumerator<object[]> GetEnumerator()
        => Options
            .Select(options =>
            {
                var skip = !string.IsNullOrWhiteSpace(Filter) && options.TestName != Filter;
                return new object[]
                {
                    options , skip
                };
            })
            //adding the first 2 tests to make sure that even if the test have been filtered, at least one test fails when a filter is applied
            // this guarantees that no tests are skipped unintentionally in the test pipeline
            .Where((parameters, i) 
                => !(bool)parameters[1] || ListSkippedTests || i == 0 || i == 1)
            .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
public abstract class ReflectionTestBase
{
    private readonly ITestOutputHelper _testOutput;
    private readonly IExceptionManager _exceptions;

    protected ReflectionTestBase(ITestOutputHelper testOutput)
    {
        _exceptions = new ExceptionManager(GetType().FullClassName());
        _testOutput = testOutput;
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Xunit(testOutput)
            .Enrich.FromLogContext()
            .MinimumLevel.Warning()
            .CreateLogger();
    }
    public virtual void Test(ReflectionTestOptions options, bool skipTest)
    {
        _testOutput.WriteLine("TestName: {0}", options.TestName);
        if (skipTest)
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