using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JLib.Testing;
using Snapshooter;
using Snapshooter.Xunit;
using JLib.Reflection.Attributes;

namespace JLib.Tests.Reflection;
/// <summary>
/// tests for multiple constellations of inputTypes for the typeCache<br/>
/// to add a testcase, create a subclass with the expected types.<br/>
/// the result will be validated via snapshot<br/>
/// to run only a single test for debug purposes, add the <see cref="FocusTestAttribute"/> to the container class
/// </summary>
public class TypeCacheTests
{

    public static class TypeValueTypeWithoutFactoryAttribute
    {
        public record InvalidTypeValueType(Type Value) : TypeValueType(Value);
    }
    public static class InconclusiveFactory
    {
        public interface IDemoTypeA { }
        public interface IDemoTypeB { }

        [TvtFactoryAttributes.Implements(typeof(IDemoTypeA))]
        public record DemoTypeValueTypeA(Type Value) : TypeValueType(Value);
        [TvtFactoryAttributes.Implements(typeof(IDemoTypeB))]
        public record DemoTypeValueTypeB(Type Value) : TypeValueType(Value);
        public class InconclusiveType : IDemoTypeA, IDemoTypeB { }
    }


    #region testing logic
    /// <summary>
    /// a parent class with this attribute will be the only test executed and cause the <see cref="TypeCacheTests.FocusNotSet"/> test to fail
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    // ReSharper disable once ClassNeverInstantiated.Local
    private class FocusTestAttribute : Attribute { }
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
                                        { "Kind", t.IsClass?"Class":t.IsValueType?"struct":t.IsInterface?"Interface":"other"},
                                        { "Attributes", t.GetCustomAttributes<Attribute>().Select(a=>a.GetType().FullClassName()) },
                                        { "ImplementedInterfaces", t.GetInterfaces().Select(t2=>t2.FullClassName()).OrderBy(i=>i)},
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
                    cache.KnownTypeValueTypes.Select(t=>t.Name).OrderBy(x=>x)
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
    #endregion
}
