using FluentAssertions;
using JLib.ValueTypes;
using Xunit;

namespace Examples;

/// <summary>
/// This example describes, how validation works when value types are derived from each other
/// </summary>
// ReSharper disable once InconsistentNaming
public class Inheriting_from_another_ValueType
{
    public record EmailAddress(string Value) : StringValueType(Value)
    {
        [Validation]
        private static void Validate(StringValidator must)
            => must.Contain("@").Contain(".");
    }
    public record GermanEmailAddress(string Value) : EmailAddress(Value)
    {
        [Validation]
        private static void Validate(StringValidator must)
            => must.EndWith(".de");
    }

    [Fact]
    public void ValidValue()
    {
        var sut = () => new GermanEmailAddress("my@example.de");
        sut.Should().NotThrow();
    }
    [Fact]
    public void InvalidValue()
    {
        var sut = () => new GermanEmailAddress("my@example.com");
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void InvalidValueCheckedByBaseType()
    {
        var sut = () => new GermanEmailAddress("example.com");
        sut.Should().Throw<AggregateException>();
    }
}