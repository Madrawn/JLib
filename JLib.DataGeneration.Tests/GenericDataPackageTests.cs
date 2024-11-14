using FluentAssertions;
using JLib.ValueTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataGeneration.Tests;

public class ValidDataPackages
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

    public abstract class Base1Dp<T> : DataPackage, IA<T>
        where T : GuidValueType
    {
        public T Id { get; init; } = null!;
        public int Id2 { get; init; }

        protected Base1Dp(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    public abstract class Base2Dp<T> : Base1Dp<DataPackageTestBase.TestTypeId>
    {
        protected Base2Dp(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    public sealed class Test3Dp : Base2Dp<object>
    {
        public Test3Dp(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }


}

public class GenericDataPackageTests : DataPackageTestBase
{


    public GenericDataPackageTests(ITestOutputHelper toh) : base(toh, TypePackage.GetNested<ValidDataPackages>())
    {
    }

    [Fact]
    public void Test1()
    {
        Provider.IncludeDataPackages<ValidDataPackages.Test3Dp>();
        var package = Provider.GetRequiredService<ValidDataPackages.Test3Dp>();
        package.Id.Should().NotBeNull();
        package.Id.IdInfo().Should().NotBeNull();
        package.GetInfoText(x => x.Id);
    }
}