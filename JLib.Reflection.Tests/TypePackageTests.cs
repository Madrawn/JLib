using System.Reflection;
using FluentAssertions;
using JLib.Helper;
using JLib.Tests.Reflection.DemoAssembly;
using JLib.Tests.Reflection.DemoAssembly2;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit;

namespace JLib.Reflection.Tests;
public class TypePackageTests
{
    #region nested classes
    private class NestingDemoClass
    {
        public class NestedDemoClassA { }
        public class NestedDemoClassB { }
        public class NestedDemoClassC { }
    }
    private class NestingDemoClass2
    {
        public class NestedDemoClass2A { }
        public class NestedDemoClass2B { }
        public class NestedDemoClass2C { }
    }
    public static IReadOnlyCollection<Type> NestedTypes => new[]
    {
        typeof(NestingDemoClass.NestedDemoClassA),
        typeof(NestingDemoClass.NestedDemoClassB),
        typeof(NestingDemoClass.NestedDemoClassC),
    };
    public static IReadOnlyCollection<Type> NestedTypes2 => new[]
    {
        typeof(NestingDemoClass2.NestedDemoClass2A),
        typeof(NestingDemoClass2.NestedDemoClass2B),
        typeof(NestingDemoClass2.NestedDemoClass2C),
    };
    #endregion
    #region direct classes
    private class DemoClassA { }
    private class DemoClassB { }
    private class DemoClassC { }
    private static readonly IReadOnlyCollection<Type> DemoTypes = new[]
    {
        typeof(DemoClassA),
        typeof(DemoClassB),
        typeof(DemoClassC),
    };
    #endregion
    #region assembly
    private static readonly IReadOnlyCollection<Type> DemoAssemblyTypes = new[]
    {
        typeof(TestAssemblyDemoClassA),
        typeof(TestAssemblyDemoClassB),
        typeof(TestAssemblyDemoClassC),
    };
    private static readonly Assembly DemoAssembly = DemoAssemblyTypes.First().Assembly;
    private static readonly IReadOnlyCollection<Type> DemoAssembly2Types = new[]
    {
        typeof(TestAssembly2DemoClassA),
        typeof(TestAssembly2DemoClassB),
        typeof(TestAssembly2DemoClassC),
    };
    private static readonly Assembly DemoAssembly2 = DemoAssembly2Types.First().Assembly;
    #endregion

    public static readonly object[][] TestCases = new object[][]
    {
        #region Assembly
        new object[]
        {
            "SingleAssemblyWithNameTemplate",
            TypePackage.Get(DemoAssembly, "Testing Assembly {0} {1}"),
            DemoAssemblyTypes
        },
        new object[]
        {
            "SingleAssembly",
            TypePackage.Get(DemoAssembly),
            DemoAssemblyTypes
        },
        new object[]
        {
            "MultiAssemblyParams",
            TypePackage.Get(DemoAssembly,DemoAssembly2),
            DemoAssemblyTypes.Concat(DemoAssembly2Types)
        },
        new object[]
        {
            "MultiAssemblyCollection",
            TypePackage.Get(new []
                {
                    DemoAssembly,
                    DemoAssembly2
                }.CastTo<IReadOnlyCollection<Assembly>>()),
            DemoAssemblyTypes.Concat(DemoAssembly2Types)
        },
        #endregion
        #region explicit type
        new object[]
        {
            "SingleTypeAssembly",
            TypePackage.Get(typeof(DemoClassA)),
            new[]{typeof(DemoClassA)},
        },
        new object[]
        {
            "MultiTypeAssemblyParams",
            TypePackage.Get(DemoTypes.ToArray()),
            DemoTypes,
        },
        new object[]
        {
            "MultiTypeAssemblyCollection",
            TypePackage.Get(DemoTypes),
            DemoTypes,
        },
        #endregion
        #region nested type
        new object[]
        {
            "NestedSingleArg",
            TypePackage.GetNested(typeof(NestingDemoClass)),
            NestedTypes,
        },
        new object[]
        {
            "NestedSingleTypeArg",
            TypePackage.GetNested<NestingDemoClass>(),
            NestedTypes,
        },
        new object[]
        {
            "NestedMultiParams",
            TypePackage.GetNested(typeof(NestingDemoClass),typeof(NestingDemoClass2)),
            NestedTypes.Concat(NestedTypes2),
        },
        #endregion
        #region Assembly and Types combined
        new object[]
        {
            "CombinedAssembliesOnly",
            TypePackage.Get(new []{DemoAssembly,DemoAssembly2},Enumerable.Empty<Type>()),
            DemoAssemblyTypes.Concat(DemoAssembly2Types)
        },
        new object[]
        {
            "CombinedTypesOnly",
            TypePackage.Get(Enumerable.Empty<Assembly>(),DemoTypes),
            DemoTypes
        },
        new object[]{
            "CombinedSource",
            TypePackage.Get(new []{DemoAssembly,DemoAssembly2},DemoTypes),
            DemoAssemblyTypes.Concat(DemoAssembly2Types).Concat(DemoTypes)
        },
        #endregion
        #region Merging
        new object[]{
            "Merged",
            TypePackage.Get(
                TypePackage.Get(typeof(DemoClassA)),
                TypePackage.Get(typeof(DemoClassB), typeof(DemoClassC))
            ),
            DemoTypes
        },
        #endregion
        #region FileSystem
        new object[]{
            "ByFileSystem",
            TypePackage.Get(null,new []{"JLib.Tests.Reflection.Demo"}),
            DemoAssemblyTypes.Concat(DemoAssembly2Types)
        },
        #endregion
    };
    [Theory, MemberData(nameof(TestCases))]
    public void PackageGeneration(
        string name, ITypePackage package, IEnumerable<Type> expectedTypes)
    {
        package.GetContent().Should().OnlyContain(t => expectedTypes.Contains(t));
        expectedTypes.Should().OnlyContain(t => package.GetContent().Contains(t));
        package.ToString().MatchSnapshot(new SnapshotNameExtension(name));
    }
}
