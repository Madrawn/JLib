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
    /// <summary>
    /// the options to provide to the test method
    /// </summary>
    protected abstract IEnumerable<ReflectionTestOptions> Options { get; }
    /// <summary>
    /// filters for test options with this exact name
    /// </summary>
    protected virtual string Filter { get; } = "";
    /// <summary>
    /// when true, skipped tests will be executed
    /// </summary>
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
                => !(bool)parameters[1] || ListSkippedTests
#if RELEASE
                                        || i == 0 || i == 1
#endif
            )
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

    public virtual void AddServices(IServiceCollection services, ITypeCache cache, IExceptionManager exceptions)
    {

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
        object cacheValidator;
        try
        {
            cacheValidator = cache.All<ITypeValueType>()
                .Where(tvt => tvt.Value.Assembly != typeof(ITypeCache).Assembly)
                .PrepareSnapshot();
        }
        catch (Exception e)
        {
            cacheValidator = e.PrepareSnapshot() ?? "evaluation failed";
        }

        AddServices(services, cache, _exceptions.CreateChild(nameof(AddServices)));
        serviceFactory(services, cache, _exceptions.CreateChild("Service Factory"));

        var exceptionValidator = _exceptions.GetException().PrepareSnapshot();

        object serviceValidator;
        try
        {
            serviceValidator = services.PrepareSnapshot();
        }
        catch (Exception e)
        {
            serviceValidator = e.PrepareSnapshot() ?? "evaluation failed";
        }

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