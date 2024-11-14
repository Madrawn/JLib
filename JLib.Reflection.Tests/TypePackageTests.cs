using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using JLib.Helper;
using JLib.Reflection.Tests.DemoAssembly1;
using JLib.Reflection.Tests.DemoAssembly2;
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

    #region Assembly
    [Fact]
    public void SingleAssemblyWithNameTemplate()
    => RunTest(
        TypePackage.Get(DemoAssembly, "Testing Assembly {0} {1}"),
        DemoAssemblyTypes
);
    [Fact]
    public void SingleAssembly()
    => RunTest(
        TypePackage.Get(DemoAssembly),
        DemoAssemblyTypes
);
    [Fact]
    public void MultiAssemblyParams()
    => RunTest(
        TypePackage.Get(DemoAssembly, DemoAssembly2),
        DemoAssemblyTypes.Concat(DemoAssembly2Types)
);
    [Fact]
    public void MultiAssemblyCollection()
    => RunTest(
        TypePackage.Get(new[]
            {
                    DemoAssembly,
                    DemoAssembly2
            }.CastTo<IReadOnlyCollection<Assembly>>()),
        DemoAssemblyTypes.Concat(DemoAssembly2Types)
);
    #endregion
    #region explicit type
    [Fact]
    public void SingleTypeAssembly()
    => RunTest(
        TypePackage.Get(typeof(DemoClassA)),
        new[] { typeof(DemoClassA) }
);
    [Fact]
    public void MultiTypeAssemblyParams()
    => RunTest(
        TypePackage.Get(DemoTypes.ToArray()),
        DemoTypes
);
    [Fact]
    public void MultiTypeAssemblyCollection()
    => RunTest(
        TypePackage.Get(DemoTypes),
        DemoTypes
);
    #endregion
    #region nested type

    [Fact]
    public void NestedSingleArg()
        => RunTest(
            TypePackage.GetNested(typeof(NestingDemoClass)),
            NestedTypes
        );
    [Fact]
    public void NestedSingleTypeArg()
        => RunTest(
            TypePackage.GetNested<NestingDemoClass>(),
            NestedTypes
    );

    [Fact]
    public void NestedMultiParams()
        => RunTest(
            TypePackage.GetNested(typeof(NestingDemoClass), typeof(NestingDemoClass2)),
            NestedTypes.Concat(NestedTypes2)
        );
    #endregion
    #region Assembly and Types combined

    [Fact]
    public void CombinedAssembliesOnly()
        => RunTest(
            TypePackage.Get(new[] { DemoAssembly, DemoAssembly2 }, Enumerable.Empty<Type>()),
            DemoAssemblyTypes.Concat(DemoAssembly2Types)
        );
    [Fact]
    public void CombinedTypesOnly()
        => RunTest(
                TypePackage.Get(Enumerable.Empty<Assembly>(), DemoTypes),
                DemoTypes
            );

    [Fact]
    public void CombinedSource()
        => RunTest(
            TypePackage.Get(new[] { DemoAssembly, DemoAssembly2 }, DemoTypes),
            DemoAssemblyTypes.Concat(DemoAssembly2Types).Concat(DemoTypes)
        );
    #endregion
    [Fact]
    public void Merged()
        => RunTest(
            TypePackage.Get(
                TypePackage.Get(typeof(DemoClassA)),
                TypePackage.Get(typeof(DemoClassB), typeof(DemoClassC))
            ),
            DemoTypes);
    [Fact]
    public void ByFileSystem()
    {
        RunTest(
            TypePackage.Get(null, new[] { "JLib.Reflection.Tests.Demo" }),
            DemoAssemblyTypes.Concat(DemoAssembly2Types));
    }

    private void RunTest(
         ITypePackage package, IEnumerable<Type> expectedTypes, [CallerMemberName] string name = "")
    {
        // .net 7 adds some attributes which are not included in any other .net version,
        // which means we have to remove them from the result to match all other versions
        package
            .GetContent()
            .Should().OnlyContain(t => expectedTypes.Contains(t)
#if NET7_0
                || new[] { "EmbeddedAttribute","RefSafetyRulesAttribute","RefSafetyRulesAttribute" }.Contains(t.Name)
#endif
            );
        expectedTypes.Should().OnlyContain(t => package.GetContent().Contains(t));
        package.ToJson()
#if NET7_0
            .Replace("├ Types:5", "├ Types:3")
            .Replace(@"
  │   EmbeddedAttribute", "")
            .Replace(@"
  │   RefSafetyRulesAttribute", "")
            .Replace(@"
│   EmbeddedAttribute", "")
            .Replace(@"
│   RefSafetyRulesAttribute", "")
#endif
            .MatchSnapshot($"{nameof(TypePackageTests)}.{name}"

            );
    }
}
