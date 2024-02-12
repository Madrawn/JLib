using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataProvider.Testing;

public abstract class ReflectionTestBase
{
    private readonly ITestOutputHelper _testOutput;
    private readonly IExceptionManager _exceptions;
    private readonly ILoggerFactory _loggerFactory;

    protected ReflectionTestBase(ITestOutputHelper testOutput)
    {
        _exceptions = new ExceptionManager(GetType().FullClassName());
        _testOutput = testOutput;
        _loggerFactory = new LoggerFactory()
            .AddXunit(testOutput);
    }

    protected virtual void AddServices(IServiceCollection services, ITypeCache cache, IExceptionManager exceptions)
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

        IServiceCollection services = new ServiceCollection()
            .AddTypeCache(
                out var cache, _exceptions, _loggerFactory, TypePackage.Get(content), JLibDataProviderTp.Instance);

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
        serviceFactory(services, cache, _loggerFactory, _exceptions.CreateChild("Service Factory"));

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