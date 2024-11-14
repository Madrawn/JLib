using FluentAssertions;
using Xunit;

namespace JLib.ValueTypes.Tests;
public class ValueTypeFactoryTests
{
    public record PositiveNumber(int Value) : IntValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<int> value)
            => value.BePositive();
    }

    public record NumericString(string Value) : StringValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<string> value)
            => value.BeNumeric();
    }

    [Fact]
    public void StructA()
    {
        ValueType.Create<PositiveNumber, int>(5).Should().Be(new PositiveNumber(5));
    }
    [Fact]
    public void StructB()
    {
        ((Action)(() => ValueType.Create<PositiveNumber, int>(-1)))
            .Should().Throw<Exception>()
        ;
    }
    [Fact]
    public void StructC()
    {
        ValueType.CreateNullable<PositiveNumber, int>(null).Should().BeNull();
    }
    [Fact]
    public void ClassA()
    {
        ValueType.Create<NumericString, string>("5").Should().Be(new NumericString("5"));
    }
    [Fact]
    public void ClassB()
    {
        ((Action)(() => ValueType.Create<NumericString, string>("A").Should()))
            .Should().Throw<Exception>();
    }
    [Fact]
    public void ClassC()
    {
        ((Action)(() => ValueType.Create<NumericString, string>(null).Should()))
            .Should().Throw<Exception>();
    }
    [Fact]
    public void ClassD()
    {
        ValueType.CreateNullable<NumericString, string>(null).Should().BeNull();
    }
}
