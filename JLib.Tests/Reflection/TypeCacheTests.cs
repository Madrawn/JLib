using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JLib.FactoryAttributes;
using JLib.Testing;
using Snapshooter;
using Snapshooter.Xunit;

namespace JLib.Tests.Reflection;
public class TypeCacheTests
{
    #region Test Environments
    /// <summary>
    /// a parent class with this attribute will be the only test executed and cause the <see cref="TypeCacheTests.FocusNotSet"/> test to fail
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    // ReSharper disable once ClassNeverInstantiated.Local
    private class FocusTestAttribute : Attribute { }

    public class TypeValueTypeWithoutFactoryAttribute
    {
        public record InvalidTypeValueType(Type Value) : TypeValueType(Value);
    }

    public class InconclusiveFactory
    {
        public interface IDemoTypeA { }
        public interface IDemoTypeB { }
        [TvtFactoryAttributes.Implements(typeof(IDemoTypeA))]
        public record DemoTypeValueTypeA { }
        [TvtFactoryAttributes.Implements(typeof(IDemoTypeB))]
        public record DemoTypeValueTypeB { }
        public class InconclusiveType : IDemoTypeA, IDemoTypeB { }
    }
    #endregion
    public TypeCacheTests()
    {
    }

    public static object[][] GetTestTypes()
    {
        var anyFocus = typeof(TypeCacheTests).GetNestedTypes().Any(t => t.HasCustomAttribute<FocusTestAttribute>());

        return typeof(TypeCacheTests)
            .GetNestedTypes()
            .Where(t => !anyFocus || t.HasCustomAttribute<FocusTestAttribute>())
            .Select(t => new object[] { t })
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(GetTestTypes))]
    public void Test(Type sut)
    {
        var testName = sut.Name;
        var package = TypePackage.GetNested(sut);
        var exceptions = new ExceptionManager(testName);
        var cache = new TypeCache(package, exceptions);
        new Dictionary<string, object?>()
            {
                { "TestName",testName },
                {
                    "Environment",
                    new Dictionary<string, object>()
                    {
                        {
                            "Types",
                            sut.GetNestedTypes().ToDictionary(t=>t.Name,
                                t => new Dictionary<string, object?>()
                                    {
                                        {"Kind",t.IsClass?"Class":t.IsValueType?"struct":t.IsInterface?"Interface":"other"},
                                        { "Attributes", t.GetCustomAttributes<Attribute>().Select(a=>a.GetType().FullClassName()) },
                                        {"ImplementedInterfaces",t.GetInterfaces().Select(t2=>t2.FullClassName())},
                                    }
                                )
                        }
                    }
                },
                {
                    "Exceptions",
                    exceptions?.GetException()?.PrepareSnapshot()
                },
                {
                    "CachedTypes",
                    cache.All<ITypeValueType>().PrepareSnapshot()
                },
                {
                    "KnownTypeValueTypes",
                    cache.KnownTypeValueTypes.Select(t=>t.Name)
                }
            }.MatchSnapshot(new SnapshotNameExtension(testName));
    }

    [Fact]
    public void FocusNotSet()
    {
        typeof(TypeCacheTests)
            .GetNestedTypes()
            .Should()
            .OnlyContain(t => !t.HasCustomAttribute<FocusTestAttribute>(true), "Some tests have the Focus attribute set");
    }
}
