using FluentAssertions;
using JLib.AutoMapper;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using JLib.ValueTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataGeneration.Tests;

public class DefaultDataPackageTests : DataPackageTestBase
{
    public DefaultDataPackageTests(ITestOutputHelper toh) : base(toh)
    {
    }
}
public class GenericDataPackageTests : DataPackageTestBase
{
    public interface IB
    {
        public Guid Id { get; }
    }
    public interface IA<out TId> : IB
        where TId : GuidValueType
    {
        public new TId Id { get; }
        Guid IB.Id => Id.Value;

    }
    public abstract class Base1Dp<T> : DataPackage,IA<T>
        where T : GuidValueType
    {
        public T Id { get; init; } = null!;
        public int Id2 { get; init; }
        protected Base1Dp(IDataPackageManager packageManager) : base(packageManager) { }
    }

    public abstract class Base2Dp<T> : Base1Dp<TestTypeId>
    {
        protected Base2Dp(IDataPackageManager packageManager) : base(packageManager)
        {
        }
    }
    public sealed class Test3Dp : Base2Dp<object>
    {
        public Test3Dp(IDataPackageManager packageManager) : base(packageManager)
        {
        }
    }

    public sealed class ThrowingDp : Base2Dp<object>
    {
        public TestTypeId Id { get; init; } = null!;
        public ThrowingDp(IDataPackageManager packageManager) : base(packageManager)
        {
        }
    }
    public GenericDataPackageTests(ITestOutputHelper toh) : base(toh)
    {
    }

    [Fact]
    public void Test1()
    {
        Provider.IncludeDataPackages<Test3Dp>();
        var package = Provider.GetRequiredService<Test3Dp>();
        package.Id.Should().NotBeNull();
        package.Id.IdInfo().Should().NotBeNull();
        package.GetInfoText(nameof(Test3Dp.Id));
    }
    [Fact]
    public void Test2()
    {
        Provider.IncludeDataPackages<ThrowingDp>();
        var package = Provider.GetRequiredService<ThrowingDp>();
        package.Id.Should().NotBeNull();
        package.Id.IdInfo().Should().NotBeNull();
        package.GetInfoText(nameof(ThrowingDp.Id));
    }
}
public abstract class DataPackageTestBase : IDisposable
{
    public record TestTypeId(Guid Value) : GuidValueType(Value);

    private readonly List<IDisposable> _disposables = new();

    public void Dispose()
        => _disposables.DisposeAll();

    protected DataPackageTestBase(ITestOutputHelper toh)
    {
        using var exceptions = new ExceptionBuilder(GetType().FullName());
        var logger = LoggerFactory.Create(x => x.AddXunit(toh));
        var services = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, logger,
                JLibDataGenerationTp.Instance,
                TypePackage.GetNested(GetType()))
            .AddAutoMapper(x => x.AddProfiles(typeCache, logger))
            .AddLogging(x => x.AddXunit(toh))
            .AddDataPackages(typeCache, new() { DefaultNamespace = "JLib.DataGeneration.Tests" });
        Provider = services.BuildServiceProvider().DisposeWith(_disposables);
    }

    protected ServiceProvider Provider { get; }
}