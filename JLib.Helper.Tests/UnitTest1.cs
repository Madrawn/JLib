using FluentAssertions;

using Xunit;

namespace JLib.Helper.Tests;

public class UnitTest1
{
    public class SubClassA<T>
    {
        public class SubClassB<T2>
        {

        }
        public class SubClassC<T2>
        {

        }
    }

    public class OtherClass
    {
        public class OtherSubClass { }
    }
    public class OtherGenericClass<T> { }
    [Fact]
    public void Test1()
    {
        var typeAB = typeof(SubClassA<OtherGenericClass<OtherClass>>
                    .SubClassB<OtherGenericClass<OtherClass.OtherSubClass>>
                    );
        var typeAC = typeof(SubClassA<OtherGenericClass<OtherClass>>
                    .SubClassC<OtherGenericClass<OtherClass.OtherSubClass>>
                    );

        typeAB.FullName().Should().NotBe(typeAC.FullName());

        //SubClassB needs to be present in the full name, otherwise the full name would be the same for both types
        typeAB.FullName().Should().Be(
            "UnitTest1.SubClassA<UnitTest1.OtherGenericClass<UnitTest1.OtherClass>>"
            + ".SubClassB<UnitTest1.OtherGenericClass<UnitTest1.OtherClass.OtherSubClass>>");

        typeAC.FullName().Should().Be(
            "UnitTest1.SubClassA<UnitTest1.OtherGenericClass<UnitTest1.OtherClass>>"
            + ".SubClassC<UnitTest1.OtherGenericClass<UnitTest1.OtherClass.OtherSubClass>>");
    }
    [Fact]
    public void Test2()
    {
        typeof(SubClassA<int>)
            .FullName().Should().Be("UnitTest1.SubClassA<Int32>");
    }
    [Fact]
    public void Test3()
    {
        typeof(OtherClass.OtherSubClass)
            .FullName().Should().Be("UnitTest1.OtherClass.OtherSubClass");
    }
    [Fact]
    public void Test4()
    {
        var typeABC = typeof(SubClassA<OtherGenericClass<OtherClass>>
                    .SubClassB<OtherGenericClass<SubClassA<OtherGenericClass<OtherClass>>
                    .SubClassC<OtherGenericClass<OtherClass.OtherSubClass>>>>
                    );
        typeABC
            .FullName(true).Should().Be(
                "JLib.Helper.Tests.UnitTest1.SubClassA"
                + "<JLib.Helper.Tests.UnitTest1.OtherGenericClass<JLib.Helper.Tests.UnitTest1.OtherClass>>"
                + ".SubClassB<JLib.Helper.Tests.UnitTest1.OtherGenericClass<JLib.Helper.Tests.UnitTest1.SubClassA"
                + "<JLib.Helper.Tests.UnitTest1.OtherGenericClass<JLib.Helper.Tests.UnitTest1.OtherClass>>"
                + ".SubClassC<JLib.Helper.Tests.UnitTest1.OtherGenericClass"
                + "<JLib.Helper.Tests.UnitTest1.OtherClass.OtherSubClass>>>>"
            );
    }

    [Fact]
    public void Test5()
    {
        typeof(OtherClass.OtherSubClass)
            .FullName().Should().NotBe(typeof(OtherRootClass.OtherClass.OtherSubClass).FullName());
    }
    [Fact]
    public void Test6()
    {
        typeof(SubClassA<OtherRootClass.OtherClass>.SubClassB<OtherClass>)
            .FullName().Should().Be("UnitTest1.SubClassA<OtherRootClass.OtherClass>.SubClassB<UnitTest1.OtherClass>");
    }
}

public class OtherRootClass
{
    public class OtherClass
    {
        public class OtherSubClass { }
    }
}