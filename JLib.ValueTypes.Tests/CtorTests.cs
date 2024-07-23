using FluentAssertions;
using Xunit;

namespace JLib.ValueTypes.Tests;

/// <summary>
/// Tests whether the constructor triggers the validation correctly
/// </summary>
public class ConstructorTests
{

    public record FiveCharacterString(string Value) : StringValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<string?> must)
            => must.BeOfLength(5);
    }

    public record PositiveInt(int Value) : IntValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<int> must)
            => must.BePositive();
    }

    [Fact]
    public void StringIsValid() 
        => new FiveCharacterString("valid").Value.Should().Be("valid");

    [Fact]
    public void StringValueIsInvalid()
    {
        var sut = () => new FiveCharacterString("invalid");
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void StringValueIsNull()
    {
        var sut = () => new FiveCharacterString(null!);
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void StructValueIsValid()
    {
        new PositiveInt(5).Value.Should().Be(5);
    }
    [Fact]
    public void StructValueIsInvalid()
    {
        var sut = () => new PositiveInt(-5);
        sut.Should().Throw<AggregateException>();
    }
}